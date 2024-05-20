import SwiftUI
import AVKit

struct VideoContentView: View {
    let videoUrlString: String
    @State private var isPlaying: Bool = false
    @State private var player: AVPlayer
    @State private var playerObserver: Any?
    @State private var isPresentingFullScreenPlayer = false // New state variable to track presentation

    init(_ videoUrlString: String) {
        self.videoUrlString = videoUrlString
        _player = State(initialValue: AVPlayer(url: Bundle.main.url(forResource: "Data/Raw/videos/" + videoUrlString, withExtension: "MOV")!))
    }

    var body: some View {
        VStack {
            SpatialVideoPlayerView(player: player)
                .frame(width: 900, height: 600) // Adjust the size as needed
                .background(Color.black)
                .cornerRadius(10)
                .shadow(radius: 10)
                .onAppear {
                    // Add observer to update play/pause state
                    playerObserver = player.addPeriodicTimeObserver(forInterval: CMTime(seconds: 1, preferredTimescale: 600), queue: .main) { _ in
                        if player.currentItem?.status == .readyToPlay {
                            isPlaying = player.rate != 0
                        }
                    }
                }
                .onDisappear {
                    // Remove observer
                    if let observer = playerObserver {
                        player.removeTimeObserver(observer)
                        playerObserver = nil
                    }
                }

            // Playback controls
            HStack {
                Button(action: {
                    let currentTime = player.currentTime()
                    let newTime = CMTimeMake(value: currentTime.value - 10 * Int64(currentTime.timescale), timescale: currentTime.timescale)
                    player.seek(to: newTime)
                }) {
                    Image(systemName: "gobackward.10")
                        .foregroundColor(.white)
                        .padding()
                }
                
                Button(action: {
                    if isPlaying {
                        player.pause()
                    } else {
                        player.play()
                    }
                    isPlaying.toggle()
                }) {
                    Image(systemName: isPlaying ? "pause.fill" : "play.fill")
                        .foregroundColor(.white)
                        .padding()
                }

                Button(action: {
                    let currentTime = player.currentTime()
                    let newTime = CMTimeMake(value: currentTime.value + 10 * Int64(currentTime.timescale), timescale: currentTime.timescale)
                    player.seek(to: newTime)
                }) {
                    Image(systemName: "goforward.10")
                        .foregroundColor(.white)
                        .padding()
                }
                // Button to present fullscreen player
                Button(action: {
                    isPresentingFullScreenPlayer = true
                }) {
                    Text("View Immersive")
                        .padding()
                        .cornerRadius(10)
                }
                .sheet(isPresented: $isPresentingFullScreenPlayer) {
                    // Present AVPlayerViewController in fullscreen mode
                    AVPlayerViewControllerWrapper(player: player)
                }
            }
            .padding()
        }
    }
    
    class CustomPlayerView: UIView {
        private let playerLayer: AVPlayerLayer

        init(player: AVPlayer) {
            self.playerLayer = AVPlayerLayer(player: player)
            super.init(frame: .zero)
            setupLayer()
        }

        required init?(coder: NSCoder) {
            fatalError("init(coder:) has not been implemented")
        }

        private func setupLayer() {
            playerLayer.videoGravity = .resizeAspect
            layer.addSublayer(playerLayer)
        }

        override func layoutSubviews() {
            super.layoutSubviews()
            playerLayer.frame = bounds
        }
    }

    struct SpatialVideoPlayerView: UIViewRepresentable {
        let player: AVPlayer

        func makeUIView(context: Context) -> CustomPlayerView {
            let view = CustomPlayerView(player: player)
            view.backgroundColor = .black
            return view
        }

        func updateUIView(_ uiView: CustomPlayerView, context: Context) {
            uiView.layoutSubviews() // Force layout update
        }
    }
    
    struct AVPlayerViewControllerWrapper: UIViewControllerRepresentable {
        let player: AVPlayer
        
        func makeUIViewController(context: Context) -> AVPlayerViewController {
            let playerViewController = AVPlayerViewController()
            playerViewController.player = player
            return playerViewController
        }
        
        func updateUIViewController(_ uiViewController: AVPlayerViewController, context: Context) {
            // Update if needed
        }
    }
}
