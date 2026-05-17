using MaruSikaku.Gameplay.Players;
using UnityEngine;

namespace MaruSikaku.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class CameraChaseController : MonoBehaviour
    {
        public const float Z_POS = -10f;

        private PlayerController _player;

        // Update is called once per frame
        void Update()
        {
            SetCameraPos();
        }

        private void SetCameraPos()
        {
            var playerPos = _player.transform.position;
            transform.position = new(playerPos.x, playerPos.y, Z_POS);
        }

        public void Initialize(PlayerController player)
        {
            SetTargetPlayer(player);
        }

        public void SetTargetPlayer(PlayerController player)
        {
            _player = player;
        }
    }
}