using UnityEngine;
using System.Collections;


public class SharedFloat : SharedVariable
{
    public float Value { get { return mValue; } set { mValue = value; } }

    private float mValue;

    public SharedFloat() { ValueType = SharedVariableTypes.Float; }

    public override object GetValue() { return mValue; }
    public override void SetValue(object value) { mValue = (float)value; }

    public override string ToString() { return mValue.ToString(); }
}
