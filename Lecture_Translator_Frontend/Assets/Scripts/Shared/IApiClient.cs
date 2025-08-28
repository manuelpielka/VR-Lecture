using System.Threading.Tasks;
using UnityEngine;

public interface IApiClient
{
    Task<string> PostRequest(string url, string json);
}