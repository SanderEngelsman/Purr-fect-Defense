using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    public void Freeze()
    {
        Time.timeScale = 0f;
    }

    public void Unfreeze()
    {
        Time.timeScale = 1f;
    }
}
