using UnityEngine;

namespace OctoberStudio
{
    public class FollowPlayerBehavior : MonoBehaviour
    {
        [SerializeField] Vector3 offset;

        void Update()
        {
            if(PlayerBehavior.Player != null)
            {
                transform.position = PlayerBehavior.Player.transform.position + offset;
            }
        }
    }
}