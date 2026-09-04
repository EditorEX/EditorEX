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

            Container.BindInterfacesAndSelfTo<HeckCustomDataCodec>().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomDataCodecRegistry>().AsSingle();
        }

        private void BindCache(string id)
        {
            Container
                .Bind<EditorDeserializedData>()
                .WithId(id)
                .FromMethod(_ => new EditorDeserializedData())
                .AsSingle();
        }
    }
}
