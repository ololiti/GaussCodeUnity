using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClicked : MonoBehaviour{
    public InputField gaussCode;
    public InputField markedCrossings;
    public Text fText;

    public void OnGoClicked(){
        ClosedCurve curve = new ClosedCurve(gaussCode.text, 
                                            markedCrossings.text);
        fText.text = curve.toString();
    }
}
