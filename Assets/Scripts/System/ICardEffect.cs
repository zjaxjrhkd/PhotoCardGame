using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    void Init(ScoreManager scoreManager, GameMaster gameMaster, CardManager cardManager);

    void Effect();
}
