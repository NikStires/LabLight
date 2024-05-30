#import <AVFoundation/AVFoundation.h>
#import <Speech/Speech.h>

void requestSpeechAuthorization(SFSpeechRecognizer *speechRecognizer, SFSpeechAudioBufferRecognitionRequest *recognitionRequest) {
    //request authorization
    [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus status) {
        if (status == SFSpeechRecognizerAuthorizationStatusAuthorized) {
            UnitySendMessage("SpeechRecognitionManager", "OnSpeechRecognitionAuthorized", "Speech recognition authorized");
        }
        else
        {
            UnitySendMessage("SpeechRecognitionManager", "OnSpeechRecognitionUnauthorized", "Speech recognition unauthorized");
        }
    }];
}

void requestSpeechRecognition(SFSpeechRecognizer *speechRecognizer, SFSpeechAudioBufferRecognitionRequest *recognitionRequest, AVAudioEngine *audioEngine) 
{
    //setup the audio session for microphone input
    AVAudioSession *audioSession = [AVAudioSession sharedInstance];
    NSError *error;
    [audioSession setCategory:AVAudioSessionCategoryRecord error:&error];
    [audioSession setActive:YES withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation error:&error];

    AVAudioInputNode *inputNode = [audioEngine inputNode];
    AVAudioFormat *format = [inputNode outputFormatForBus:0];
    [inputNode installTapOnBus:0 bufferSize:1024 format:format block:^(AVAudioPCMBuffer *buffer, AVAudioTime *when) {
        [recognitionRequest appendAudioPCMBuffer:buffer];
    }];

    [audioEngine prepare];
    [audioEngine startAndReturnError:&error];

    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(5.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        // Wait for 5 seconds for the audio session to finish taking microphone input
        [audioEngine stop];
        [recognitionRequest endAudio];
        
        // Start the recognition task and return the result
        [speechRecognizer recognitionTaskWithRequest:recognitionRequest resultHandler:^(SFSpeechRecognitionResult *result, NSError *error) {
            if (result) {
                NSString *transcription = [[result bestTranscription] formattedString];
                // Send the transcription back to Unity
                UnitySendMessage("SpeechRecognitionManager", "OnSpeechRecognitionResult", [transcription UTF8String]);
            }
            
            if (error) {
                // Send the error back to Unity
                UnitySendMessage("SpeechRecognitionManager", "OnSpeechRecognitionError", [[error localizedDescription] UTF8String]);
            }
        }];
    });
}

extern "C" 
{
    void _requestSpeechAuthorization()
    {
        SFSpeechRecognizer *speechRecognizer = [[SFSpeechRecognizer alloc] initWithLocale:[[NSLocale alloc] initWithLocaleIdentifier:@"en-US"]];
        SFSpeechAudioBufferRecognitionRequest *recognitionRequest = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
        
        requestSpeechAuthorization(speechRecognizer, recognitionRequest);
    }

    void _requestSpeechRecognition()
    {
        SFSpeechRecognizer *speechRecognizer = [[SFSpeechRecognizer alloc] initWithLocale:[[NSLocale alloc] initWithLocaleIdentifier:@"en-US"]];
        SFSpeechAudioBufferRecognitionRequest *recognitionRequest = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
        AVAudioEngine *audioEngine = [[AVAudioEngine alloc] init];
        
        requestSpeechRecognition(speechRecognizer, recognitionRequest, audioEngine);
    }
}
