using UnityEngine;

public class GUIHelper : MonoBehaviour 
{
    public Texture icon;

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 430, 120), "Ghost Free Roam Camera!");
        GUI.DrawTexture(new Rect(20, 20, 32, 32), icon);
        GUI.Label(new Rect(20, 52, 500, 500), "Now you can have a free fly camera in your game, or for debugging!\n" +
                                              "Use the mouse to turn\n" +
                                              "Use W,A,S,D to move\n" +
                                              "Use Escape to toggle the cursor (maximize on play in editor)");
    }
}
