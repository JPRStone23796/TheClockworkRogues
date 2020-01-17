using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFightRoomTrigger
{

    void ClosePrimary(bool Close, float time);

    void CloseSecondary(bool Close, float time);

    void setRoomManager(GameObject managerObject);


}
