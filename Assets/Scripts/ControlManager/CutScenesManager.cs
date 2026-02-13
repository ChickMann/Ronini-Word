using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class CutScenesManager : MonoBehaviour
{
        [SerializeField] private PlayableDirector finisherSuccess;

        [SerializeField] private string _enemyTrackName = "EnemyTrack";
        
        private Action _currentOnCompleteCallback;

        private void OnEnable()
        {
                finisherSuccess.stopped += OnDirectorStopped;

        }

        private void OnDisable()
        {
                finisherSuccess.stopped -= OnDirectorStopped;
        }
        /// <summary>
        /// Gọi hàm này khi bắt đầu Finisher
        /// </summary>
        /// <param name="enemyInstance">Con quái thực tế đang đứng trong game</param>
        public void PlayCinematic(PlayableDirector _director,Actor enemyInstance, Action onCompleted = null)
        {
                
                _currentOnCompleteCallback = onCompleted;
                // 1. Lấy Timeline Asset
                var timelineAsset = (TimelineAsset)_director.playableAsset;

// 2. Lấy Animator cần gán
                var enemyAnimator = enemyInstance.GetComponent<Animator>();

// 3. LỌC TRACK: Dùng .Where() để lấy tất cả track có tên trùng khớp
                var targetTracks = timelineAsset.GetOutputTracks()
                        .Where(t => t.name == _enemyTrackName);

// Kiểm tra xem có tìm thấy track nào không (Optional)
                if (!targetTracks.Any())
                {
                        Debug.LogError($"Không tìm thấy track nào tên '{_enemyTrackName}' trong Timeline!");
                        return;
                }

// 4. Duyệt qua danh sách và gán binding
                foreach (var track in targetTracks)
                {
                        // Lệnh quan trọng: Gán Animator vào Track
                        _director.SetGenericBinding(track, enemyAnimator);
                }
              
                
                _director.playableAsset = timelineAsset;
                _director.time = 0;

                // 5. Sau khi gán xong xuôi thì mới chạy phim
                _director.Play();
        }

        public void PlayFinisherSuccess(float timeStart,Actor enemy, Action action =null)
        {
                this.DelayAction(timeStart, (() =>
                {
                        PlayCinematic(finisherSuccess, enemy, action);
                }));
        }
        /// <summary>
        /// Lên lịch thực hiện một hành động tại thời điểm cụ thể của Timeline
        /// </summary>
        /// <param name="timeInSeconds">Thời điểm muốn kích hoạt (giây)</param>
        /// <param name="action">Hàm cần chạy</param>
        public void ScheduleTimelineAction(float timeInSeconds, Action action)
        {
                if (finisherSuccess == null || action == null) return;

                // Nếu timeline đã chạy qua điểm đó rồi thì gọi luôn cho khỏi hụt
                if (finisherSuccess.time >= timeInSeconds)
                {
                        action.Invoke();
                        return;
                }

                StartCoroutine(WaitForTimeRoutine(timeInSeconds, action));
        }

        private IEnumerator WaitForTimeRoutine(double targetTime, Action action)
        {
                // BƯỚC 1 (FIX LỖI CŨ): Đợi cho đến khi Director thực sự bắt đầu chạy
                // Nếu gọi hàm này trước khi Play(), nó sẽ nằm chờ ở đây chứ không thoát.
                while (finisherSuccess.state != PlayState.Playing)
                {
                        yield return null;
                }

                // BƯỚC 2: Theo dõi thời gian
                // Dùng 'double' cho targetTime vì Director.time dùng độ chính xác kép
                while (finisherSuccess.time < targetTime)
                {
                        // Case dự phòng: Nếu Timeline bị Stop giữa chừng (User skip hoặc game over)
                        // thì hủy theo dõi để không bị kẹt Coroutine vĩnh viễn.
                        if (finisherSuccess.state != PlayState.Playing) 
                        {
                                yield break; // Hủy lệnh
                        }

                        yield return null;
                }

                // BƯỚC 3: Kích hoạt
                action?.Invoke();
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
