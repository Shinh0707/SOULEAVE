using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyCodeImage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyCodeTextMesh;

    protected KeyCode _keyCode;

    public KeyCode KeyCode
    {
        get { return _keyCode; }
        set {
            if (_keyCode != value)
            {
                keyCodeTextMesh.text = _keyCode.ToString();
                _keyCode = value;
            }
        }
    } 
}
