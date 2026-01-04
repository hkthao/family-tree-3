Refactor my Python audio pipeline with the following philosophy:

- Backend should NOT enhance or denoise audio
- Focus only on validating and scoring audio quality
- Preserve original speech characteristics
- Avoid any processing that may change voice identity

Goals:
- Detect whether audio contains real human speech
- Reject low-quality or unusable audio
- Standardize format only (mono, 16kHz, WAV)

Allowed steps:
- Resampling / format conversion
- Voice Activity Detection
- Basic loudness / clipping checks

Disallowed:
- Noise reduction
- Audio enhancement
- Aggressive normalization

Return:
- A simplified pipeline
- A function that returns a quality report (pass / warn / reject)
- Clear explanation for each check
