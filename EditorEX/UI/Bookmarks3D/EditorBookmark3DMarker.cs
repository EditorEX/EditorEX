using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditorEX.UI.Bookmarks3D
{
    internal class EditorBookmark3DMarker : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MeshRenderer _wallRenderer = null!;
        private TextMeshPro _label = null!;
        private BoxCollider _labelCollider = null!;
        private MaterialPropertyBlock? _propertyBlock;

        public float Beat { get; private set; }

        public BoxCollider LabelCollider => _labelCollider;

        public static EditorBookmark3DMarker Create(
            Transform parent,
            Mesh quadMesh,
            Material wallMaterial,
            Quaternion wallRotation,
            Vector3 wallScale,
            float wallLocalY,
            TMP_FontAsset font,
            Material fontMaterial
        )
        {
            var root = new GameObject("EditorEXBookmark3DMarker");
            root.transform.SetParent(parent, false);

            var marker = root.AddComponent<EditorBookmark3DMarker>();
            marker.Build(
                quadMesh,
                wallMaterial,
                wallRotation,
                wallScale,
                wallLocalY,
                font,
                fontMaterial
            );
            return marker;
        }

        public void SetData(string text, Color color, float beat)
        {
            Beat = beat;

            var wallColor = color;
            wallColor.a = 1f;
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.SetColor(ColorId, wallColor);
            _wallRenderer.SetPropertyBlock(_propertyBlock);

            _label.text = string.IsNullOrWhiteSpace(text) ? "Bookmark" : text;
            _label.color = new Color(color.r, color.g, color.b, 1f);
            _label.ForceMeshUpdate();
        }

        public void SetWorldZ(float z)
        {
            var position = transform.localPosition;
            position.z = z;
            transform.localPosition = position;
        }

        private void Build(
            Mesh quadMesh,
            Material wallMaterial,
            Quaternion wallRotation,
            Vector3 wallScale,
            float wallLocalY,
            TMP_FontAsset font,
            Material fontMaterial
        )
        {
            var quadObject = new GameObject("Wall");
            quadObject.transform.SetParent(transform, false);
            quadObject.transform.localPosition = new Vector3(0f, wallLocalY, 0f);
            quadObject.transform.localRotation = wallRotation;
            quadObject.transform.localScale = wallScale;

            var filter = quadObject.AddComponent<MeshFilter>();
            filter.sharedMesh = quadMesh;

            _wallRenderer = quadObject.AddComponent<MeshRenderer>();
            _wallRenderer.sharedMaterial = wallMaterial;
            _wallRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _wallRenderer.receiveShadows = false;
            _wallRenderer.lightProbeUsage = LightProbeUsage.Off;
            _wallRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _wallRenderer.allowOcclusionWhenDynamic = false;

            float labelX = Mathf.Abs(wallScale.x) * 0.5f + 0.05f;
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(labelX, 0.35f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            _label = labelObject.AddComponent<TextMeshPro>();
            _label.font = font;
            _label.fontSharedMaterial = fontMaterial;
            _label.fontSize = 3f;
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            _label.overflowMode = TextOverflowModes.Ellipsis;
            _label.raycastTarget = false;
            _label.rectTransform.pivot = new Vector2(0f, 0.5f);
            _label.rectTransform.sizeDelta = new Vector2(2.6f, 0.55f);

            _labelCollider = labelObject.AddComponent<BoxCollider>();
            _labelCollider.center = new Vector3(1.3f, 0f, 0f);
            _labelCollider.size = new Vector3(2.6f, 0.55f, 0.12f);
        }
    }
}
