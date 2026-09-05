using System.Collections.Generic;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorHeckAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<Dictionary<string, Track>>().AsSingle();

            BindCache("Heck");
            BindCache("NoodleExtensions");
            BindCache("Chroma");
            BindCache("Vivify");

            Container.BindInterfacesAndSelfTo<HeckCustomDataCodec>().AsCached();
            Container.BindInterfacesAndSelfTo<CustomDataCodecRegistry>().AsSingle();
        }

        private void BindCache(string id)
        {
            Container.BindInstance(new EditorDeserializedData()).WithId(id);
        }
    }
}
