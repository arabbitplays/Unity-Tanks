using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LayerMaskReference
{
    public bool useConstant = true;
    public LayerMask constantValue;
    public LayerMaskVariable variable;

    public LayerMask value {
        get { return useConstant ? constantValue : variable.value; }
        set {
            if (useConstant) 
                constantValue = value;
            else 
                variable.value = value;
        }
    }
}
