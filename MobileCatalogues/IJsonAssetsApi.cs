using System.Collections.Generic;

namespace MobileCatalogues
{
    public interface IJsonAssetsApi
    {
        IDictionary<string, int> GetAllCropIds();
    }
}
