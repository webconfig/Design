using System;
using UnityEngine;

public class SharedVariable : ScriptableObject
{
    public bool IsShared;

    public SharedVariableTypes ValueType;

    public virtual object GetValue() { return null; }

    public virtual void SetValue(object value) { }
}
public enum SharedVariableTypes
{
    Int,
    Float,
    Bool,
    String,
    Vector2,
    Vector3,
    Vector4,
    Quaternion,
    Color,
    Rect,
    GameObject,
    Transform,
    Object
}
