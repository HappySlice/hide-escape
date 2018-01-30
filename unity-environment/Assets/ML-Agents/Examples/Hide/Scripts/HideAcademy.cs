using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAcademy : Academy
{
    public enum Mode
    {
        Hide, Escape
    }

    public Mode mode;
    public float chaserSpeed;

    public override void AcademyReset()
    {
        chaserSpeed = (float)resetParameters["chaser_speed"];
    }

    public override void AcademyStep()
    {

    }
}
