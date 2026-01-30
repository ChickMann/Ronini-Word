using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutScenesManager : MonoBehaviour
{
        [SerializeField] private PlayableDirector finisherSuccess;
        [SerializeField] private PlayableDirector finisherFail;

        [SerializeField] private string _enemyTrackName = "EnemyTrack";
        
        private Action _currentOnCompleteCallback;

        private void OnEnable()
        {
                finisherFail.stopped += OnDirectorStopped;
                finisherSuccess.stopped += OnDirectorStopped;

        }

        private void OnDisable()
        {
                finisherFail.stopped -= OnDirectorStopped;
                finisherSuccess.stopped -= OnDirectorStopped;
        }
        /// <summary>
        /// Gọi hàm này khi bắt đầu Finisher
        /// </summary>
        /// <param name="enemyInstance">Con quái thực tế đang đứng trong game</param>
        public void PlayCinematic(PlayableDirector _director,Actor enemyInstance, Action onCompleted = null)
        {
                
                _currentOnCompleteCallback = onCompleted;
                // 1. Lấy kịch bản gốc (TimelineAsset) từ Director
                var timelineAsset = (TimelineAsset)_director.playableAsset;
                
                // 2. Tìm cái Track có tên là "EnemyTrack"
                // (Dùng LINQ để tìm cho nhanh và gọn)
                var enemyTrack = timelineAsset.GetOutputTracks()
                        .FirstOrDefault(t => t.name == _enemyTrackName);

                if (enemyTrack != null)
                {
                        // 3. Lấy Animator của con quái
                        var enemyAnimator = enemyInstance.GetComponent<Animator>();

                        // 4. LỆNH QUAN TRỌNG NHẤT: Gán Animator của quái vào Track
                        _director.SetGenericBinding(enemyTrack, enemyAnimator);
                }
                else
                {
                        Debug.LogError($"Không tìm thấy track tên {_enemyTrackName} trong Timeline!");
                }
                
                _director.playableAsset = timelineAsset;
                _director.time = 0;

                // 5. Sau khi gán xong xuôi thì mới chạy phim
                _director.Play();
        }

        public void PlayFinisherSuccess(Actor enemy, Action action =null)
        {
                PlayCinematic(finisherSuccess,enemy,action);
        }
        public void PlayFinisherFail(Actor enemy,Action action = null)
        {
                PlayCinematic(finisherFail,enemy,action);
        }


        private void OnDirectorStopped(PlayableDirector director)
        {
                // Khi phim dừng, kiểm tra xem có việc gì được dặn dò không
                if (_currentOnCompleteCallback != null)
                {
                        _currentOnCompleteCallback.Invoke(); // Thực hiện hành động!
                        _currentOnCompleteCallback = null;   // Xóa đi để không gọi nhầm lần sau
                }
        }

}
