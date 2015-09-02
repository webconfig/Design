using UnityEngine;
using System.Collections;

public class SharedGameObject : SharedVariable
{
    public GameObject Value { get { return mValue; } set { mValue = value; } }

    private GameObject mValue;

    public SharedGameObject() { ValueType = SharedVariableTypes.GameObject; }

    public override object GetValue() { return mValue; }
    public override void SetValue(object value) { mValue = (GameObject)value; }

    public override string ToString() { return (mValue == null ? "null" : mValue.name); }
}
