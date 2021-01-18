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
        int GetObjectId(string name);

        IDictionary<string, int> GetAllObjectIds();
        IDictionary<string, int> GetAllCropIds();
    }
}
