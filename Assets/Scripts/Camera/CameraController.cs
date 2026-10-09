using UnityEngine;

namespace NeonSurvivor
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<CameraController>();
                return instance;
            }
        }

        static CameraController instance;

        [SerializeField] Transform target;
        [SerializeField] float smoothSpeed = 12f;
        [SerializeField] float fixedZ = -10f;

        Vector3 basePosition;
        Vector3 shakeOffset;
        float shakeMagnitude;
        float shakeTime;
        float shakeDuration = 0.16f;

        public static void ClearStatics()
        {
            instance = null;
        }

        void OnEnable()
        {
            instance = this;
            basePosition = new Vector3(transform.position.x, transform.position.y, fixedZ);
        }

        void OnDisable()
        {
            if (instance == this)
                instance = null;
        }

        void LateUpdate()
        {
            if (target == null && PlayerController.Instance != null)
                target = PlayerController.Instance.transform;

            Vector3 desired = basePosition;
            if (target != null)
                desired = new Vector3(target.position.x, target.position.y, fixedZ);

            float t = Mathf.Clamp01(smoothSpeed * Time.deltaTime);
            basePosition = Vector3.Lerp(basePosition, desired, t);

            if (shakeTime > 0f)
            {
                shakeTime -= Time.unscaledDeltaTime;
                float damp = shakeDuration <= 0.001f ? 0f : Mathf.Clamp01(shakeTime / shakeDuration);
                shakeOffset = (Vector3)(Random.insideUnitCircle * shakeMagnitude * damp);
                if (shakeTime <= 0f)
                {
                    shakeOffset = Vector3.zero;
                    shakeMagnitude = 0f;
                }
            }

            transform.position = basePosition + shakeOffset;
        }

        public void EnsureTarget(Transform newTarget)
        {
            if (target == null)
                target = newTarget;
        }

        public void Shake(float amount)
        {
            shakeMagnitude = Mathf.Max(shakeMagnitude, amount);
            shakeDuration = 0.16f;
            shakeTime = shakeDuration;
        }
    }
}
