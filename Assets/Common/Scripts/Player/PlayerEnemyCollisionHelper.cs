using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class PlayerEnemyCollisionHelper : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerBehavior.Player.CheckTriggerEnter2D(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            PlayerBehavior.Player.CheckTriggerExit2D(collision);
        }
    }
}