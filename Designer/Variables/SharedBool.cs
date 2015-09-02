using UnityEngine;
using System.Collections;


public class SharedBool : SharedVariable
{
    public bool Value { get { return mValue; } set { mValue = value; } }
    [SerializeField]
    private bool mValue;

    public SharedBool() { ValueType = SharedVariableTypes.Bool; }

    public override object GetValue() { return mValue; }
    public override void SetValue(object value) { mValue = (bool)value; }

    public override string ToString() { return mValue.ToString(); }
}
