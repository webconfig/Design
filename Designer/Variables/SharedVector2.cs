using UnityEngine;
using System.Collections;


[System.Serializable]
public class SharedVector2 : SharedVariable
{
    public Vector2 Value { get { return mValue; } set { mValue = value; } }
    [SerializeField]
    private Vector2 mValue;

    public SharedVector2() { ValueType = SharedVariableTypes.Vector2; }

    public override object GetValue() { return mValue; }
    public override void SetValue(object value) { mValue = (Vector2)value; }

    public override string ToString() { return mValue.ToString(); }
}
