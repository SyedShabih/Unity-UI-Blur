using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SimpleUIBlur
{
    public class UIBlur : MonoBehaviour
    {
        //Down Samples the captured image to 1/2, 1/4 or 1/8 resoultion
        [Range(0, 3)]
        public int downsample = 1;

        //Use Standard Gauss (Performance) or Sgx Gauss (Quality). Recommend Using Standard Gaussian Blur
        public enum BlurPreference
        {
            StandardGauss = 0,
            SgxGauss = 1,
        }

        //Captures Either screenshot or targets camera. If you want camera to blur UI layer as well then set camera as screen space camera and assign to main camera
        public enum BlurType
        {
            Static = 0,
            Dynamic = 1,
        }

        //Determine the Blur Spread
        [Range(0.0f, 10.0f)]
        public float blurSize = 3.0f;

        //Number of iterations to repeat blur on a single image.
        [Range(1, 5)]
        public int blurIterations = 2;

        [SerializeField] private BlurPreference blurPref = BlurPreference.StandardGauss;

        //Base Blur Material, Must have Shader UI/FastBlur
        [SerializeField] private Material blurMaterial = null;

        //The Target Image
        [SerializeField] private RawImage blurLayer;


        private Texture2D screenGrab;
        private RenderTexture source;
        private RenderTexture destination;
        private BlurType blurType;
        private bool scheduleUpdate = false;

        private void RenderImage()
        {
            blurLayer.enabled = false;
            float widthMod = 1.0f / (1.0f * (1 << downsample));

            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
            source.filterMode = FilterMode.Bilinear;

            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;
            destination = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            // downsample
            RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

            rt.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, rt, blurMaterial, 0);

            var passOffs = blurPref == BlurPreference.StandardGauss ? 0 : 2;

            for (int i = 0; i < blurIterations; i++)
            {
                float iterationOffs = (i * 1.0f);
                blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

                // vertical blur
                RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, blurMaterial, 1 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, blurMaterial, 2 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            Graphics.Blit(rt, destination);
            blurMaterial.SetTexture("_MainTex", destination);
            blurLayer.enabled = true;
            RenderTexture.ReleaseTemporary(rt);
        }

        public void EnableBlur(BlurType bt)
        {
            blurType =bt;
            if (blurType == BlurType.Static)
            {
                StartCoroutine(ScreenCaptureCoroutine());
            }
            else if (blurType == BlurType.Dynamic)
            {
                if (!GetComponent<Camera>())
                {
                    throw new Exception("Please Add This Script to the camera you want to target for dynamic blur to work");
                }
                scheduleUpdate = true;
            }
        }

        private IEnumerator ScreenCaptureCoroutine()
        {
            yield return new WaitForEndOfFrame();
            screenGrab = ScreenCapture.CaptureScreenshotAsTexture();
            source = RenderTexture.GetTemporary(screenGrab.width, screenGrab.height, 0);
            Graphics.Blit(screenGrab, source);
            DestroyImmediate(screenGrab);
            RenderImage();
        }

        public void DisableBlur()
        {
            if (blurType == BlurType.Static)
            {
                StopAllCoroutines();
                RenderTexture.ReleaseTemporary(source);
            }
            else
            {
                scheduleUpdate = false;
                source = null;
            }
            RenderTexture.ReleaseTemporary(destination);
            blurLayer.enabled = false;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (scheduleUpdate)
            {
                source = src;
                RenderImage();
            }
            Graphics.Blit(src, dest);
        }
    }
}
