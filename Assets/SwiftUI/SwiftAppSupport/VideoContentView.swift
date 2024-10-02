import SwiftUI
import AVKit

struct VideoContentView: View {
    let videoUrlString: String
    @State private var isPlaying: Bool = false
    @State private var player: AVPlayer?
    @State private var playerObserver: Any?
    @State private var isPresentingFullScreenPlayer = false
    @State private var errorMessage: String?

    init(_ videoUrlString: String) {
        self.videoUrlString = videoUrlString
        _player = State(initialValue: nil)
    }

    var body: some View {
        VStack {
            if let player = player {
                SpatialVideoPlayerView(player: player)
                    .frame(width: 900, height: 600)
                    .background(Color.black)
                    .cornerRadius(10)
                    .shadow(radius: 10)
                    .onAppear {
                        setupPlayerObserver()
                    }
                    .onDisappear {
                        removePlayerObserver()
                    }

                // Playback controls
                HStack {
                    // ... (existing playback control buttons)
                }
                .padding()
            } else if let errorMessage = errorMessage {
                Text(errorMessage)
                    .foregroundColor(.red)
                    .padding()
            } else {
                ProgressView()
                    .padding()
            }
        }
        .onAppear {
            setupPlayer()
        }
    }

    private func setupPlayer() {
        guard let url = Bundle.main.url(forResource: "Data/Raw/videos/" + videoUrlString, withExtension: "MOV") else {
            errorMessage = "Video file not found"
            return
        }
        player = AVPlayer(url: url)
    }

    private func setupPlayerObserver() {
        guard let player = player else { return }
        playerObserver = player.addPeriodicTimeObserver(forInterval: CMTime(seconds: 1, preferredTimescale: 600), queue: .main) { _ in
            if player.currentItem?.status == .readyToPlay {
                isPlaying = player.rate != 0
            }
        }
    }

    private func removePlayerObserver() {
        if let observer = playerObserver, let player = player {
            player.removeTimeObserver(observer)
            playerObserver = nil
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
