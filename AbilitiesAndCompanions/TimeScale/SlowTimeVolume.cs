using System;
using System.Collections;
using System.Collections.Generic;

public class SlowTimeVolume : VolumeController
{
    private void OnEnable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged += OnSlowTimeChanged;
    }
    private void OnDisable()
    {
        SlowTimeLilTosiAbility.OnSlowTimeChanged -= OnSlowTimeChanged;
    }
    private void OnSlowTimeChanged(bool obj)
    {
        if (obj)
            Activate();
        else
            Deactivate();
    }
}
