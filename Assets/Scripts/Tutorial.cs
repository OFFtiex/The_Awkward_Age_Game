using UnityEngine.UI;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject tmp;
    public Image tutorial;

    private float transparencyValue = 0;
    private bool toFadeOut = false;
    private bool toFadeIn = true;

    private void Start(){
        tutorial = GetComponent<Image>();
    }

    private void Update(){
        if(tmp == null && toFadeIn){
            transparencyValue += Time.deltaTime * 2f;             
            tutorial.color = new Color(1, 1, 1, transparencyValue);
            if (tutorial.color.a >= 1f){
                toFadeIn = false;
                Invoke("StartFadeOut", 4f);
            }
        }
        if(toFadeOut){
            transparencyValue -= Time.deltaTime * 2f;             
            tutorial.color = new Color(255, 255, 255, transparencyValue);
            if (tutorial.color.a <= 0){
                toFadeOut = false;
            }
        }
    }

    private void StartFadeOut(){
        toFadeOut = true;
    }
}
