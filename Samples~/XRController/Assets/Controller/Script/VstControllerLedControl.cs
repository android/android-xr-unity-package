using System.Collections;
using UnityEngine;

namespace XrRigResource
{
    public class VstControllerLedControl : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color initColor = new(0f, 0f, 0f, 1f);
 
        
        private static readonly int MaterialColorProperty = Shader.PropertyToID("_Color");
        private Color _currentColor;
        private Material _ledMaterial;
        private Coroutine _colorTransitionCoroutine;


        private void Awake()
        {
            _ledMaterial = meshRenderer.material;
            _currentColor = initColor;
            _ledMaterial.SetColor(MaterialColorProperty, initColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetColor"></param>
        /// <param name="transitionTime"></param>
        public void SetStaticColor(Color targetColor, float transitionTime)
        {
            StopCurrentCoroutine();
            _colorTransitionCoroutine = StartCoroutine(ChangeColor(_currentColor, targetColor, transitionTime));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="transitionTime"></param>
        public void SetBlinkColor(Color startColor, Color endColor, float transitionTime)
        {
            StopCurrentCoroutine();
            _colorTransitionCoroutine = StartCoroutine(BlinkColor(startColor, endColor, transitionTime));
        }


        private IEnumerator ChangeColor(Color currentColor, Color targetColor, float transitionTime)
        {
            float passedTime = 0f;

            while (true)
            {
                if (passedTime >= transitionTime)
                {
                    _currentColor = targetColor;
                    _ledMaterial.SetColor(MaterialColorProperty, _currentColor);
                    yield break;
                }
                
                float percentage = passedTime / transitionTime;
                _currentColor = Color.Lerp(currentColor, targetColor, percentage);
                _ledMaterial.SetColor(MaterialColorProperty, _currentColor);
                passedTime += Time.deltaTime;

                yield return null;
            }
        }


        private IEnumerator BlinkColor(Color startColor, Color endColor, float transitionTime, int loopCount = 0)
        {
            _currentColor = startColor;
            float passedTime = 0f;
            Color startLerpColor = startColor;
            Color nextLerpColor = endColor;
            int loop = 0;

            Color GetNextColor(Color currentColor)
            {
                if (currentColor == startColor) return endColor;
                return startColor;
            }

            while (true)
            {
                if (passedTime >= transitionTime)
                {
                    _currentColor = nextLerpColor;
                    _ledMaterial.SetColor(MaterialColorProperty, _currentColor);
                    startLerpColor = _currentColor;
                    nextLerpColor = GetNextColor(_currentColor);
                    passedTime = 0f;
                    loop++;
                    
                    if(loopCount > 0 && loop == loopCount)
                    {
                        yield break;
                    }
                }
                
                float percentage = passedTime / transitionTime;
                _currentColor = Color.Lerp(startLerpColor, nextLerpColor, percentage);
                _ledMaterial.SetColor(MaterialColorProperty, _currentColor);
                passedTime += Time.deltaTime;
                
                yield return null;
            }
        }


        private void StopCurrentCoroutine()
        {
            if (_colorTransitionCoroutine != null)
            {
                StopCoroutine(_colorTransitionCoroutine);
            }
        }


        public void Reset()
        {
            _currentColor = initColor;
            _ledMaterial.SetColor(MaterialColorProperty, initColor);
        }
    }
}
