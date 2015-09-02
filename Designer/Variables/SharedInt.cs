using UnityEngine;
using System.Collections;


[System.Serializable]
public class SharedInt : SharedVariable
{
    public int Value { get { return mValue; } set { mValue = value; } }
    [SerializeField]
    private int mValue;

    public SharedInt() { ValueType = SharedVariableTypes.Int; }

    public override object GetValue() { return mValue; }
    public override void SetValue(object value) { mValue = (int)value; }

    public override string ToString() { return mValue.ToString(); }
}
