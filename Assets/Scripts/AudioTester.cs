using UnityEditor;
using UnityEngine;


    public class AudioTester : MonoBehaviour
    {
        // Place your AudioSource component you want to test here
        // Place your audioclip inside of that source and adjust your settings in inspector
        [Tooltip("Audiosource you want to test goes here. Adjust its settings as desired.")]
        [SerializeField] AudioSource audioSource;

        // Functions to test the audio (can be called from the inspector without going into Play Mode.)
        public void PlayAudio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                if(audioSource.pitch < 0)
                    audioSource.pitch *= -1;
                audioSource.timeSamples = 0;
                audioSource.Play();
            }
        }

        public void PlayReverseAudio()
        {
            if(audioSource != null)
            {
                audioSource.Stop();
                if(audioSource.pitch > 0)
                    audioSource.pitch *= -1;
                audioSource.timeSamples = audioSource.clip.samples - 1;
                audioSource.Play();
            }
        }

        public void PauseUnpauseAudio()
        {
            if(audioSource != null)
            {
                if(audioSource.isPlaying)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
            }
        }

        public void StopAudio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }

    // Playing audio without going into Play Mode requires a custom inspector.
    [CustomEditor(typeof(AudioTester))]
    public class AudioTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector first
            DrawDefaultInspector();

            // Reference to the script instance
            AudioTester AudioTester = (AudioTester)target;

        // Adds some space between test buttons and the serialized box
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();  // This makes the play and reverse play buttons are in the same row, purely cosmetic
        GUILayout.FlexibleSpace(); // Makes it so space to the left is staying flexy, nice looking.

        GUILayoutOption buttonWidth = GUILayout.Width(250); // For matching button widths so its even and nice :)

            // Add a button to play the audio, duh :D
            if(GUILayout.Button("Play Audio", buttonWidth))
            {
                AudioTester.PlayAudio();
            }

        GUILayout.Space(10);

            // Creating custom button content for the sake of adding a custom tooltip when your mouse hovers the button. GUIContent(insertButtonLabel, tooltipGoesHere)
            GUIContent reverseButtonContent = new GUIContent("Play Reversed Audio", 
                "To play reverse audio we first need to set the timesample to the end of the audio before playing." +
                "Then make sure the pitch is negative to go in reverse. -1 is the default speed.");

            if(GUILayout.Button(reverseButtonContent, buttonWidth))
            {
                AudioTester.PlayReverseAudio();
            }

        GUILayout.FlexibleSpace(); // Makes sure the space to the right of this row is also staying flexy, heckin nice again.
        EditorGUILayout.EndHorizontal(); // End of the row

            GUIContent pauseButtonContent = new GUIContent("Pause / Unpause Audio", 
                "This does not reset the audio, simply pauses it where you heard it.");

            if(GUILayout.Button(pauseButtonContent))
            {
                AudioTester.PauseUnpauseAudio();
            } 

            GUIContent stopButtonContent = new GUIContent("Stop Audio", 
                "The AudioSource.stop function stops the currently set Audio clip from playing. " +
                "The Audio clip plays from the beginning the next time you play it.");

            if(GUILayout.Button(stopButtonContent))
            {
                AudioTester.StopAudio();
            }
        }
    }