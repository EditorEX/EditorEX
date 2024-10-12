using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.CustomDataModels
{
    public class LevelCustomDataModel
    {
        public string LevelAuthorName { get; set; }

        public void UpdateWith(string levelAuthorName = null)
        {
            LevelAuthorName = levelAuthorName ?? LevelAuthorName;
        }
    }
}
