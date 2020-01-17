using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoom
{


    void BeginLevel();
    void AddEnemy(GameObject mEnemy);
    void RemoveEnemy(GameObject mDeadEnemy);
    bool CheckIsPlayerTargetable(bool mPlayer1);
    void updatePlayer1AgentValue(bool mEnemyDead);
    void updatePlayer2AgentValue(bool mEnemyDead);



}
