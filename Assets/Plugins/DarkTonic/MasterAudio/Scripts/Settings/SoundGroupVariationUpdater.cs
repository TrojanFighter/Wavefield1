/*! \cond PRIVATE */

using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    /// <summary>
    /// This class is only activated when you need code to execute in an Update method, such as "follow" code.
    /// </summary>
    // ReSharper disable once CheckNamespace
    [AudioScriptOrder(-15)]
    public class SoundGroupVariationUpdater : MonoBehaviour {
        private const float FakeNegativeFloatValue = -10f;

        private Transform _objectToFollow;
        private GameObject _objectToFollowGo;
        private bool _isFollowing;
        private SoundGroupVariation _variation;
        private float _priorityLastUpdated = FakeNegativeFloatValue;
        private bool _useClipAgePriority;
        private WaitForSoundFinishMode _waitMode = WaitForSoundFinishMode.None;
        private float _soundPlayTime;
        private AudioSource _varAudio;
        private MasterAudioGroup _parentGrp;
        private Transform _trans;
        private int _frameNum = -1;

        // fade in out vars
        private float _fadeOutStartTime = -5;
        private bool _fadeInOutWillFadeOut;
        private bool _hasFadeInOutSetMaxVolume;
        private float _fadeInOutInFactor;
        private float _fadeInOutOutFactor;

        // fade out early vars
        private int _fadeOutEarlyTotalFrames;
        private float _fadeOutEarlyFrameVolChange;
        private int _fadeOutEarlyFrameNumber;
        private float _fadeOutEarlyOrigVol;

        // gradual fade vars
        private float _fadeToTargetFrameVolChange;
        private int _fadeToTargetFrameNumber;
        private float _fadeToTargetOrigVol;
        private int _fadeToTargetTotalFrames;
        private float _fadeToTargetVolume;
        private bool _fadeOutStarted;
        private float _lastFrameClipTime = -1f;
        private float _fxTailEndTime = -1f;
        private bool _isPlayingBackward;

        private bool _hasStartedNextInChain;

        private bool _isWaitingForQueuedOcclusionRay;
        private int _framesPlayed = 0;

        private static int _maCachedFromFrame = -1;
        private static MasterAudio _maThisFrame;
        private static Transform _listenerThisFrame;

        private enum WaitForSoundFinishMode {
            None,
            Delay,
            Play,
            WaitForEnd,
            StopOrRepeat,
            FxTailWait
        }

        #region Public methods

        public void FadeOverTimeToVolume(float targetVolume, float fadeTime) {
            GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.GradualFade;

            var volDiff = targetVolume - VarAudio.volume;

            var currentClipTime = VarAudio.time;
            var currentClipLength = VarAudio.clip.length;

            if (!VarAudio.loop && VarAudio.clip != null && fadeTime + currentClipTime > currentClipLength) {
                // if too long, fade out faster
                fadeTime = currentClipLength - currentClipTime;
            }

            _fadeToTargetTotalFrames = (int)(fadeTime / AudioUtil.FrameTime);
            _fadeToTargetFrameVolChange = volDiff / _fadeToTargetTotalFrames;
            _fadeToTargetFrameNumber = 0;
            _fadeToTargetOrigVol = VarAudio.volume;
            _fadeToTargetVolume = targetVolume;
        }

        public void FadeOutEarly(float fadeTime) {
            GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeOutEarly;
            // cancel the FadeInOut loop, if it's going.

            if (!VarAudio.loop && VarAudio.clip != null && VarAudio.time + fadeTime > VarAudio.clip.length) {
                // if too long, fade out faster
                fadeTime = VarAudio.clip.length - VarAudio.time;
            }

            var frameTime = AudioUtil.FrameTime;
            if (frameTime == 0) {
                frameTime = AudioUtil.FixedDeltaTime;
            }

            _fadeOutEarlyTotalFrames = (int)(fadeTime / frameTime);
            _fadeOutEarlyFrameVolChange = -VarAudio.volume / _fadeOutEarlyTotalFrames;
            _fadeOutEarlyFrameNumber = 0;
            _fadeOutEarlyOrigVol = VarAudio.volume;
        }

        public void FadeInOut() {
            GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeInOut;
            // wait to set this so it stops the previous one if it's still going.
            _fadeOutStartTime = VarAudio.clip.length - (GrpVariation.fadeOutTime * VarAudio.pitch);

            if (GrpVariation.fadeInTime > 0f) {
                VarAudio.volume = 0f; // start at zero volume
                _fadeInOutInFactor = GrpVariation.fadeMaxVolume / GrpVariation.fadeInTime;
            } else {
                _fadeInOutInFactor = 0f;
            }

            _fadeInOutWillFadeOut = GrpVariation.fadeOutTime > 0f && !VarAudio.loop;

            if (_fadeInOutWillFadeOut) {
                _fadeInOutOutFactor = GrpVariation.fadeMaxVolume / (VarAudio.clip.length - _fadeOutStartTime);
            } else {
                _fadeInOutOutFactor = 0f;
            }
        }

        public void FollowObject(bool follow, Transform objToFollow, bool clipAgePriority) {
            _isFollowing = follow;

            if (objToFollow != null) {
                _objectToFollow = objToFollow;
                _objectToFollowGo = objToFollow.gameObject;
            }
            _useClipAgePriority = clipAgePriority;

            UpdateCachedObjects();
            UpdateAudioLocationAndPriority(false); // in case we're not following, it should get one update.
        }

        public void WaitForSoundFinish(float delaySound) {
            if (MasterAudio.IsWarming) {
                PlaySoundAndWait();
                return;
            }

            _waitMode = WaitForSoundFinishMode.Delay;

            var waitTime = 0f;

            if (GrpVariation.useIntroSilence && GrpVariation.introSilenceMax > 0f) {
                var rndSilence = Random.Range(GrpVariation.introSilenceMin, GrpVariation.introSilenceMax);
                waitTime += rndSilence;
            }

            if (delaySound > 0f) {
                waitTime += delaySound;
            }

            if (waitTime == 0f) {
                _waitMode = WaitForSoundFinishMode.Play; // skip delay mode
            } else {
                _soundPlayTime = AudioUtil.Time + waitTime;
                GrpVariation.IsWaitingForDelay = true;
            }
        }

        public void StopFading() {
            GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;

            DisableIfFinished();
        }

        public void StopWaitingForFinish() {
            _waitMode = WaitForSoundFinishMode.None;
            GrpVariation.curDetectEndMode = SoundGroupVariation.DetectEndMode.None;

            DisableIfFinished();
        }

        public void StopFollowing() {
            _isFollowing = false;
            _useClipAgePriority = false;
            _objectToFollow = null;
            _objectToFollowGo = null;

            DisableIfFinished();
        }

        #endregion

        #region Helper methods

        private void DisableIfFinished() {
            if (_isFollowing
                || GrpVariation.curDetectEndMode == SoundGroupVariation.DetectEndMode.DetectEnd
                || GrpVariation.curFadeMode != SoundGroupVariation.FadeMode.None) {

                return;
            }

            enabled = false;
        }

        private void UpdateAudioLocationAndPriority(bool rePrioritize) {
            // update location, only if following.
            if (_isFollowing && _objectToFollow != null) {
                Trans.position = _objectToFollow.position;
            }

            // re-set priority, still used by non-following (audio clip age priority)
            if (!_maThisFrame.prioritizeOnDistance || !rePrioritize || ParentGroup.alwaysHighestPriority) {
                return;
            }

            if (Time.realtimeSinceStartup - _priorityLastUpdated <= MasterAudio.ReprioritizeTime) {
                return;
            }

            AudioPrioritizer.Set3DPriority(GrpVariation, _useClipAgePriority);
            _priorityLastUpdated = AudioUtil.Time;
        }

        private void ResetToNonOcclusionSetting() {
            var lp = GrpVariation.LowPassFilter;
            if (lp != null) {
                lp.cutoffFrequency = AudioUtil.DefaultMinOcclusionCutoffFrequency;
            }
        }

        private void UpdateOcclusion() {
            var hasOcclusionOn = GrpVariation.UsesOcclusion;
            if (!hasOcclusionOn) {
                MasterAudio.StopTrackingOcclusionForSource(GrpVariation.GameObj);
                ResetToNonOcclusionSetting();

                return;
            }

            if (_listenerThisFrame == null) {
                // cannot occlude without something to raycast at.
                return;
            }

            if (IsOcclusionMeasuringPaused) {
                return; // wait until processed
            }

            MasterAudio.AddToQueuedOcclusionRays(this);
            _isWaitingForQueuedOcclusionRay = true;
        }

        private void DoneWithOcclusion() {
            _isWaitingForQueuedOcclusionRay = false;
            MasterAudio.RemoveFromOcclusionFrequencyTransitioning(GrpVariation);
        }

        /// <summary>
        /// This method is called in a batch from ListenerFollower
        /// </summary>
        /// <returns></returns>
        public bool RayCastForOcclusion() {
            DoneWithOcclusion();

            var raycastOrigin = Trans.position;

            var offset = RayCastOriginOffset;
            if (offset > 0) {
                raycastOrigin = Vector3.MoveTowards(raycastOrigin, _listenerThisFrame.position, offset);
            }

            var direction = _listenerThisFrame.position - raycastOrigin;
            var distanceToListener = direction.magnitude;

            if (distanceToListener > VarAudio.maxDistance) {
                // out of hearing range, no reason to calculate occlusion.
                MasterAudio.AddToOcclusionOutOfRangeSources(GrpVariation.GameObj);
                ResetToNonOcclusionSetting();
                return false;
            }

            MasterAudio.AddToOcclusionInRangeSources(GrpVariation.GameObj);
            var is2DRaycast = _maThisFrame.occlusionRaycastMode == MasterAudio.RaycastMode.Physics2D;

            if (GrpVariation.LowPassFilter == null) {
                // in case Occlusion got turned on during runtime.
                var newFilter = GrpVariation.gameObject.AddComponent<AudioLowPassFilter>();
                GrpVariation.LowPassFilter = newFilter;
            }

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
            // no option for this
			if (is2DRaycast) { }
#else
            var oldQueriesStart = Physics2D.queriesStartInColliders;
            if (is2DRaycast) {
                Physics2D.queriesStartInColliders = _maThisFrame.occlusionIncludeStartRaycast2DCollider;
            }

            var oldRaycastsHitTriggers = true;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (is2DRaycast) {
                oldRaycastsHitTriggers = Physics2D.queriesHitTriggers;
            } else {
                oldRaycastsHitTriggers = Physics.queriesHitTriggers;
            }
#endif

            var hitPoint = Vector3.zero;
            float? hitDistance = null;
            var isHit = false;

            if (_maThisFrame.occlusionUseLayerMask) {
                switch (_maThisFrame.occlusionRaycastMode) {
                    case MasterAudio.RaycastMode.Physics3D:
                        RaycastHit hitObject;
                        if (Physics.Raycast(raycastOrigin, direction, out hitObject, distanceToListener, _maThisFrame.occlusionLayerMask.value)) {
                            isHit = true;
                            hitPoint = hitObject.point;
                            hitDistance = hitObject.distance;
                        }

                        break;
                    case MasterAudio.RaycastMode.Physics2D:
                        var castHit2D = Physics2D.Raycast(raycastOrigin, direction, distanceToListener, _maThisFrame.occlusionLayerMask.value);
                        if (castHit2D.transform != null) {
                            isHit = true;
                            hitPoint = castHit2D.point;
                            hitDistance = castHit2D.distance;
                        }

                        break;
                }
            } else {
                switch (_maThisFrame.occlusionRaycastMode) {
                    case MasterAudio.RaycastMode.Physics3D:
                        RaycastHit hitObject;
                        if (Physics.Raycast(raycastOrigin, direction, out hitObject, distanceToListener)) {
                            isHit = true;
                            hitPoint = hitObject.point;
                            hitDistance = hitObject.distance;
                        }

                        break;
                    case MasterAudio.RaycastMode.Physics2D:
                        var castHit2D = Physics2D.Raycast(raycastOrigin, direction, distanceToListener);
                        if (castHit2D.transform != null) {
                            isHit = true;
                            hitPoint = castHit2D.point;
                            hitDistance = castHit2D.distance;
                        }

                        break;
                }
            }

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
#else
            if (is2DRaycast) {
                Physics2D.queriesStartInColliders = oldQueriesStart;
                Physics2D.queriesHitTriggers = oldRaycastsHitTriggers;
            } else {
                Physics.queriesHitTriggers = oldRaycastsHitTriggers;
            }
#endif

            if (_maThisFrame.occlusionShowRaycasts) {
                var endPoint = isHit ? hitPoint : _listenerThisFrame.position;
                var lineColor = isHit ? Color.red : Color.green;
                Debug.DrawLine(raycastOrigin, endPoint, lineColor, .1f);
            }

            if (!isHit) {
                // ReSharper disable once PossibleNullReferenceException
                MasterAudio.RemoveFromBlockedOcclusionSources(GrpVariation.GameObj);
                ResetToNonOcclusionSetting();
                return true;
            }

            MasterAudio.AddToBlockedOcclusionSources(GrpVariation.GameObj);

            var ratioToEdgeOfSound = hitDistance.Value / VarAudio.maxDistance;
            var filterFrequency = AudioUtil.GetOcclusionCutoffFrequencyByDistanceRatio(ratioToEdgeOfSound, this);

            var fadeTime = _maThisFrame.occlusionFreqChangeSeconds;
            if (fadeTime <= MasterAudio.InnerLoopCheckInterval) { // fast, just do it instantly.
                // ReSharper disable once PossibleNullReferenceException
                GrpVariation.LowPassFilter.cutoffFrequency = filterFrequency;
                return true;
            }

            MasterAudio.GradualOcclusionFreqChange(GrpVariation, fadeTime, filterFrequency);

            return true;
        }

        private void PlaySoundAndWait() {
            GrpVariation.IsWaitingForDelay = false;
            if (VarAudio.clip == null) { // in case the warming sound is an "internet file"
                return;
            }

            VarAudio.Play();
            AudioUtil.ClipPlayed(VarAudio.clip, GrpVariation.GameObj);

            if (GrpVariation.useRandomStartTime) {
                var offset = Random.Range(GrpVariation.randomStartMinPercent, GrpVariation.randomStartMaxPercent) * 0.01f * VarAudio.clip.length;
                VarAudio.time = offset;
            }

            GrpVariation.LastTimePlayed = AudioUtil.Time;

            // sound play worked! Duck music if a ducking sound.
            MasterAudio.DuckSoundGroup(ParentGroup.GameObjectName, VarAudio);

            _isPlayingBackward = GrpVariation.OriginalPitch < 0;
            _lastFrameClipTime = _isPlayingBackward ? VarAudio.clip.length + 1 : -1f;

            _waitMode = WaitForSoundFinishMode.WaitForEnd;
        }

        private void StopOrChain() {
            var playSnd = GrpVariation.PlaySoundParm;

            var wasPlaying = playSnd.IsPlaying;
            var usingChainLoop = wasPlaying && playSnd.IsChainLoop;

            if (!VarAudio.loop || usingChainLoop) {
                GrpVariation.Stop();
            }

            if (!usingChainLoop) {
                return;
            }
            StopWaitingForFinish();

            MaybeChain();
        }

        public void MaybeChain() {
            if (_hasStartedNextInChain) {
                return;
            }

            _hasStartedNextInChain = true;

            var playSnd = GrpVariation.PlaySoundParm;

            var clipsRemaining = MasterAudio.RemainingClipsInGroup(ParentGroup.GameObjectName);
            var totalClips = MasterAudio.VoicesForGroup(ParentGroup.GameObjectName);

            if (clipsRemaining == totalClips) {
                ParentGroup.FireLastVariationFinishedPlay();
            }

            // check if loop count is over.
            if (ParentGroup.chainLoopMode == MasterAudioGroup.ChainedLoopLoopMode.NumberOfLoops && ParentGroup.ChainLoopCount >= ParentGroup.chainLoopNumLoops) {
                // done looping
                return;
            }

            var rndDelay = playSnd.DelaySoundTime;
            if (ParentGroup.chainLoopDelayMin > 0f || ParentGroup.chainLoopDelayMax > 0f) {
                rndDelay = Random.Range(ParentGroup.chainLoopDelayMin, ParentGroup.chainLoopDelayMax);
            }

            // cannot use "AndForget" methods! Chain loop needs to check the status.
            if (playSnd.AttachToSource || playSnd.SourceTrans != null) {
                if (playSnd.AttachToSource) {
					MasterAudio.PlaySound3DFollowTransform(playSnd.SoundType, playSnd.SourceTrans,
                        playSnd.VolumePercentage, playSnd.Pitch, rndDelay, null, true);
                } else {
					MasterAudio.PlaySound3DAtTransform(playSnd.SoundType, playSnd.SourceTrans, playSnd.VolumePercentage,
                        playSnd.Pitch, rndDelay, null, true);
                }
            } else {
				MasterAudio.PlaySound(playSnd.SoundType, playSnd.VolumePercentage, playSnd.Pitch, rndDelay, null, true);
            }
        }

        private void PerformFading() {
            switch (GrpVariation.curFadeMode) {
                case SoundGroupVariation.FadeMode.None:
                    break;
                case SoundGroupVariation.FadeMode.FadeInOut:
                    if (!VarAudio.isPlaying) {
                        break;
                    }

                    var clipTime = VarAudio.time;
                    if (GrpVariation.fadeInTime > 0f && clipTime < GrpVariation.fadeInTime) {
                        // fade in!
                        VarAudio.volume = clipTime * _fadeInOutInFactor;
                    } else if (clipTime >= GrpVariation.fadeInTime && !_hasFadeInOutSetMaxVolume) {
                        VarAudio.volume = GrpVariation.fadeMaxVolume;
                        _hasFadeInOutSetMaxVolume = true;
                        if (!_fadeInOutWillFadeOut) {
                            StopFading();
                        }
                    } else if (_fadeInOutWillFadeOut && clipTime >= _fadeOutStartTime) {
                        // fade out!
                        if (GrpVariation.PlaySoundParm.IsChainLoop && !_fadeOutStarted) {
                            MaybeChain();
                            _fadeOutStarted = true;
                        }
                        VarAudio.volume = (VarAudio.clip.length - clipTime) * _fadeInOutOutFactor;
                    }
                    break;
                case SoundGroupVariation.FadeMode.FadeOutEarly:
                    if (!VarAudio.isPlaying) {
                        break;
                    }

                    _fadeOutEarlyFrameNumber++;

                    VarAudio.volume = (_fadeOutEarlyFrameNumber * _fadeOutEarlyFrameVolChange) + _fadeOutEarlyOrigVol;

                    if (_fadeOutEarlyFrameNumber >= _fadeOutEarlyTotalFrames) {
                        GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;
	                    GrpVariation.Stop();
                    }

                    break;
                case SoundGroupVariation.FadeMode.GradualFade:
                    if (!VarAudio.isPlaying) {
                        break;
                    }

                    _fadeToTargetFrameNumber++;
                    if (_fadeToTargetFrameNumber >= _fadeToTargetTotalFrames) {
                        VarAudio.volume = _fadeToTargetVolume;
                        StopFading();
                    } else {
                        VarAudio.volume = (_fadeToTargetFrameNumber * _fadeToTargetFrameVolChange) + _fadeToTargetOrigVol;
                    }
                    break;
            }
        }

        #endregion

        #region MonoBehavior events

        // ReSharper disable once UnusedMember.Local
        private void OnEnable() {
            // values to be reset every time a sound plays.
            _fadeInOutWillFadeOut = false;
            _hasFadeInOutSetMaxVolume = false;
            _fadeOutStarted = false;
            _hasStartedNextInChain = false;
            _framesPlayed = 0;

            DoneWithOcclusion();
            MasterAudio.RegisterUpdaterForUpdates(this);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable() {
            if (MasterAudio.AppIsShuttingDown) {
                return; // do nothing
            }

            _framesPlayed = 0;

            DoneWithOcclusion();
            MasterAudio.UnregisterUpdaterForUpdates(this);
        }

        public void UpdateCachedObjects() {
            _frameNum = AudioUtil.FrameCount;

            // ReSharper disable once InvertIf
            if (_maCachedFromFrame >= _frameNum) {
                return; // same frame. Use cached objects
            }

            // new frame. Update cached objects and frame counters;
            _maCachedFromFrame = _frameNum;
            _maThisFrame = MasterAudio.Instance;
            _listenerThisFrame = MasterAudio.ListenerTrans;
        }

        /// <summary>
        /// This method will be called by MasterAudio.cs either during LateUpdate (default) or FixedUpdate, however you've configured it in Advanced Settings.
        /// </summary>
        public void ManualUpdate() {
            UpdateCachedObjects();

            _framesPlayed++;

            if (_isFollowing) { // check for despawned caller and act if so.
                if (ParentGroup.targetDespawnedBehavior != MasterAudioGroup.TargetDespawnedBehavior.None) {
                    if (_objectToFollowGo == null || !DTMonoHelper.IsActive(_objectToFollowGo)) {
                        switch (ParentGroup.targetDespawnedBehavior) {
                            case MasterAudioGroup.TargetDespawnedBehavior.Stop:
                                GrpVariation.Stop();
                                break;
                            case MasterAudioGroup.TargetDespawnedBehavior.FadeOut:
                                GrpVariation.FadeOutNow(ParentGroup.despawnFadeTime);
                                break;
                        }

                        StopFollowing();
                    }
                }
            }

            // fade in out / out early etc.
            PerformFading();

            // priority
            UpdateAudioLocationAndPriority(true);

            // occlusion
            UpdateOcclusion();

            switch (_waitMode) {
                case WaitForSoundFinishMode.None:
                    break;
                case WaitForSoundFinishMode.Delay:
                    if (Time.realtimeSinceStartup >= _soundPlayTime) {
                        _waitMode = WaitForSoundFinishMode.Play;
                    }
                    break;
                case WaitForSoundFinishMode.Play:
                    PlaySoundAndWait();
                    break;
                case WaitForSoundFinishMode.WaitForEnd:
                    var willChangeModes = false;

                    if (_isPlayingBackward) {
                        if (VarAudio.time > _lastFrameClipTime) {
                            willChangeModes = true;
                        }
                    } else {
                        if (VarAudio.time < _lastFrameClipTime) {
                            willChangeModes = true;
                        }
                    }

                    _lastFrameClipTime = VarAudio.time;

                    if (willChangeModes) {
                        if (GrpVariation.fxTailTime > 0f) {
                            _waitMode = WaitForSoundFinishMode.FxTailWait;
                            _fxTailEndTime = AudioUtil.Time + GrpVariation.fxTailTime;
                        } else {
                            _waitMode = WaitForSoundFinishMode.StopOrRepeat;
                        }
                    }
                    break;
                case WaitForSoundFinishMode.FxTailWait:
                    if (Time.realtimeSinceStartup >= _fxTailEndTime) {
                        _waitMode = WaitForSoundFinishMode.StopOrRepeat;
                    }
                    break;
                case WaitForSoundFinishMode.StopOrRepeat:
                    StopOrChain();
                    break;
            }
        }

        #endregion

        #region Properties

        public int FramesPlayed {
            get {
                return _framesPlayed;
            }
        }

        public MasterAudio MAThisFrame {
            get {
                return _maThisFrame;
            }
        }

        public float MaxOcclusionFreq {
            get {
                // ReSharper disable once InvertIf
                if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideFrequencies) {
                    return ParentGroup.occlusionMaxCutoffFreq;
                }

                return _maThisFrame.occlusionMaxCutoffFreq;
            }
        }

        public float MinOcclusionFreq {
            get {
                // ReSharper disable once InvertIf
                if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideFrequencies) {
                    return ParentGroup.occlusionMinCutoffFreq;
                }

                return _maThisFrame.occlusionMinCutoffFreq;
            }
        }

        private Transform Trans {
            get {
                if (_trans != null) {
                    return _trans;
                }

                _trans = GrpVariation.Trans;

                return _trans;
            }
        }

        private AudioSource VarAudio {
            get {
                if (_varAudio != null) {
                    return _varAudio;
                }

                _varAudio = GrpVariation.VarAudio;

                return _varAudio;
            }
        }

        private MasterAudioGroup ParentGroup {
            get {
                if (_parentGrp != null) {
                    return _parentGrp;
                }

                _parentGrp = GrpVariation.ParentGroup;

                return _parentGrp;
            }
        }

        private SoundGroupVariation GrpVariation {
            get {
                if (_variation != null) {
                    return _variation;
                }

                _variation = GetComponent<SoundGroupVariation>();

                return _variation;
            }
        }

        private float RayCastOriginOffset {
            get {
                // ReSharper disable once InvertIf
                if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideRaycastOffset) {
                    return ParentGroup.occlusionRayCastOffset;
                }

                return _maThisFrame.occlusionRayCastOffset;
            }
        }

        private bool IsOcclusionMeasuringPaused {
            get { return _isWaitingForQueuedOcclusionRay || MasterAudio.IsOcclusionFreqencyTransitioning(GrpVariation); }
        }

        #endregion
    }
}
/*! \endcond */
