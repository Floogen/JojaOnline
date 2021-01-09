using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JojaOnline.JojaOnline.API
{
    public interface IJsonAssetApi
    {
        List<string> GetAllObjectsFromContentPack(string cp);
        IDictionary<string, int> GetAllObjectIds();
        int GetObjectId(string name);
    }
}
