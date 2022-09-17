using UnityEngine;
using SimpleUIBlur;
using UnityEngine.UI;

public class UIBlurDemo : MonoBehaviour
{
    [SerializeField] private UIBlur uiBlur;
    [SerializeField] private GameObject cube;

    private void Update()
    {
        cube.transform.Rotate(new Vector3(1,0,1), 30*Time.deltaTime);
    }

    public void PreviewStaticBlur()
    {
        if(Time.timeScale >0)
        {
            Time.timeScale = 0;
            uiBlur.EnableBlur(UIBlur.BlurType.Static);
        }
    }

    public void PreviewDynamicBlur()
    {
        if (Time.timeScale > 0)
        {
            uiBlur.EnableBlur(UIBlur.BlurType.Dynamic);
        }
    }

    public void DisableBlur()
    {
        Time.timeScale = 1;
        uiBlur.DisableBlur();
    }

}
