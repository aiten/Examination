namespace Service.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public class ServiceHelper
{
  
    public static async Task<string> GenerateUniqueRegistrationCodeAsync(Func<string,Task<bool>> exists)
    {
        var    rng = Random.Shared;
        string code;
        do
        {
            code = rng.Next(10000, 100000).ToString();
        } while (await exists(code));

        return code;
    }

}