using System.IO;
using UnityEngine;

namespace VirtualTexture
{
    public class CreatePlanesWithTexture : MonoBehaviour
    {
        [SerializeField]
        private Material material;
        private void Start()
        {
            var planePosition = new Vector3(0.0f, 0.0f, 0.0f);
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = planePosition;
            plane.transform.localScale = Vector3.one * 10.0f;
            const string texturesDir = "Assets/VirtualTexture/TestTextures";
            var texturePath = Path.Combine(texturesDir, "0-0-0.png");
            if (File.Exists(texturePath))
            {
                var fileData = File.ReadAllBytes(texturePath);
                var texture = new Texture2D(512, 512);
                texture.LoadImage(fileData);
                var planeRenderer = plane.GetComponent<Renderer>();
                planeRenderer.material = material;
                planeRenderer.material.mainTexture = texture;
            }
            else
            {
                Debug.LogError($"Texture not found at path: {texturePath}");
            }
        }
    }
}
