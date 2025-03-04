using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "ScriptableObjects/ColorData", order = 1)]
public class SaveColorData : ScriptableObject
{
    public Color savedColor = Color.white; // ค่าเริ่มต้นเป็นสีขาว
}
