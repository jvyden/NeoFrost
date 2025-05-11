using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using FrooxEngine;
using HarmonyLib;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Stream = System.IO.Stream;

namespace NeoFrost.Patches;

#nullable disable

// we don't define HarmonyPatch here as that causes an exception because we use generics here
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CloudXInterfaceRunRequestPatch
{
    public static async Task<CloudResult<T>> RunRequest<T>(CloudXInterface @this,
        Func<HttpRequestMessage> requestSource,
        TimeSpan? timeout, bool throwOnError)
        where T : class
    {
        DateTime start = DateTime.UtcNow;

        HttpRequestMessage req = null;
        HttpResponseMessage resp = null;
        HttpStatusCode status;
        
        int remainingRetries = CloudXInterface.DEFAULT_RETRIES;
        int nextRequestDelay = 250;
        
        bool success = false;
        Exception exception = null;
        do
        {
            try
            {
                req?.Dispose();
                req = requestSource();
                CancellationTokenSource cancellationTokenSource = new(timeout ?? CloudXInterface.DefaultTimeout);
                if (CloudXInterface.DEBUG_REQUESTS)
                    UniLog.Log($"{req.Method} - {req.RequestUri}");
                resp = await @this.HttpClient.SendAsync(req, cancellationTokenSource.Token).ConfigureAwait(false);
                success = true;
                if (CloudXInterface.DEBUG_REQUESTS)
                    UniLog.Log($"RESULT for {req.Method} - {req.RequestUri}:\n{resp.StatusCode}");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            
            status = resp?.StatusCode ?? 0;
            if (resp == null || resp.StatusCode == (HttpStatusCode)429 || resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (resp == null)
                {
                    UniLog.Log(
                        $"Exception running {req?.Method} request to {req?.RequestUri}. Remaining retries: {remainingRetries}. Elapsed: {GetRequestDuration()}\n" +
                        exception);
                    
                }
                else if (resp.StatusCode is HttpStatusCode.InternalServerError
                         or HttpStatusCode.BadGateway
                         or HttpStatusCode.ServiceUnavailable
                         or HttpStatusCode.GatewayTimeout)
                {
                    UniLog.Log(
                        $"Server Error running {req?.Method} request to {req?.RequestUri}. Remaining retries: {remainingRetries}. Elapsed: {GetRequestDuration()}");
                }
                
                success = false;
                
                await Task.Delay(nextRequestDelay).ConfigureAwait(false);
                nextRequestDelay *= 2;
                nextRequestDelay = MathX.Min(15000, nextRequestDelay);
            }
        } while (!success && remainingRetries-- > 0);

        if (resp == null)
        {
            req?.Dispose();
            if (!throwOnError)
                return new CloudResult<T>(default, 0);
            if (exception == null)
                throw new Exception(
                    $"Failed to get response. Last status code: {status}, Exception is null. Elapsed: {GetRequestDuration()}");
            throw exception;
        }

        T entity = default;
        string content = null;
        if (resp.IsSuccessStatusCode)
        {
            if (req.RequestUri.OriginalString.Contains(CloudXInterface.NEOS_API))
                LastLocalServerResponse.Invoke(@this, [DateTime.UtcNow]);

            if (typeof(T) == typeof(string))
            {
                content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                entity = content as T;
            }
            else
            {
                try
                {
                    long? contentLength = resp.Content.Headers.ContentLength;
                    if ((contentLength.GetValueOrDefault() > 0) & contentLength.HasValue || !resp.Content.Headers.ContentLength.HasValue)
                    {
                        using Stream responseStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);

                        if (Mappings.NeosToResonite.ContainsKey(typeof(T)))
                        {
                            object resoniteObject = await JsonSerializer
                                .DeserializeAsync(responseStream, Mappings.NeosToResonite[typeof(T)])
                                .ConfigureAwait(false);

                            entity = (T)Mappings.MapObjectToNeos(resoniteObject!);
                        }
                        else
                        {
                            entity = await JsonSerializer
                                .DeserializeAsync<T>(responseStream)
                                .ConfigureAwait(false);
                        }
                    }

                    if (CloudXInterface.DEBUG_REQUESTS)
                        UniLog.Log($"ENTITY for {req.Method} - {req.RequestUri}:\n" +
                                   JsonSerializer.Serialize(entity));
                }
                catch (Exception ex)
                {
                    UniLog.Log($"Exception deserializing {typeof(T)} response from {req.Method}:{req.RequestUri}\n" +
                               $"Exception:\n{ex}" +
                               $"\nRaw Content:\n{await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
        }
        else
        {
            content = await resp.Content.ReadAsStringAsync();
            if (CloudXInterface.DEBUG_REQUESTS)
                UniLog.Log($"CONTENT for {req.Method} - {req.RequestUri}:\n{content}");
        }

        CloudResult<T> cloudResult = new(entity, resp.StatusCode, content);
        resp.Dispose();
        req?.Dispose();
        return cloudResult;

        string GetRequestDuration()
        {
            return $"{(DateTime.UtcNow - start).TotalSeconds:F2}s";
        }
    }
    
    private static readonly MethodInfo LastLocalServerResponse = AccessTools.PropertySetter(typeof(CloudXInterface), "LastLocalServerResponse");

    [HarmonyPrefix]
    public static bool RunRequestPrefix<T>(ref Task<CloudResult<T>> __result, Func<HttpRequestMessage> requestSource,
        TimeSpan? timeout, bool throwOnError)
        where T : class
    {
        __result = RunRequest<T>(Engine.Current.Cloud, requestSource, timeout, throwOnError);
        return false;
    }

    public static void Patch(Harmony harmony)
    {
        MethodInfo originalMethod = typeof(CloudXInterface)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(m => m.Name == "RunRequest" && m.ContainsGenericParameters);

        MethodInfo prefixMethod = typeof(CloudXInterfaceRunRequestPatch)
            .GetMethod(nameof(RunRequestPrefix), BindingFlags.Static | BindingFlags.Public)!;

        // only patch types we know we're mapping.
        // types not included in that list are called vanilla-style
        // meaning behavior changes in the above patch can cause unexpected results
        foreach (Type type in Mappings.NeosTypes)
        {
            Type[] types = [type]; // to avoid double params alloc

            MethodInfo originalGenericMethod = originalMethod.MakeGenericMethod(types);
            MethodInfo prefixGenericMethod = prefixMethod.MakeGenericMethod(types);

            harmony.Patch(originalGenericMethod, new HarmonyMethod(prefixGenericMethod));
        }
    }
}