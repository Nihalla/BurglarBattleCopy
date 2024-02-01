// Author: Christy Dwyer (ChristyDwyer)

using System;
using UnityEngine;

public interface IElectricDevice
{
    public void RefreshConnections();

    public IElectricDevice GetPowerSource();

    public bool GetPowered();

    public void SetPowerSource(IElectricDevice powerSource);

    public void SetPowered(bool power);

    public void SetPoweredDownstream(bool power);
}
