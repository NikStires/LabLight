import SwiftUI
import ARKit

class MainCameraViewModel: ObservableObject {
    @Published var cameraImage: CGImage?
    private var cameraManager = MainCameraAccessManager()

    init() {
        cameraManager.onFrameProcessed = { [weak self] cgImage in
            DispatchQueue.main.async {
                self?.cameraImage = cgImage
            }
        }
        cameraManager.setupAndStart()
    }
}

struct MainCameraContentView: View {
    @StateObject private var viewModel = MainCameraViewModel()

    var body: some View {
        VStack {
            if let image = viewModel.cameraImage {
                Image(image, scale: 1.0, label: Text("Main Camera"))
                    .resizable()
                    .aspectRatio(contentMode: .fit)
            } else {
                Text("Camera feed loading...")
            }
        }
    }
}

class MainCameraAccessManager: NSObject, ObservableObject {
    private var session: ARKitSession?
    private var cameraFrameProvider: CameraFrameProvider?
    var onFrameProcessed: ((CGImage) -> Void)?

    func setupAndStart() {
        cameraFrameProvider = CameraFrameProvider()
        session = ARKitSession()

        Task {
            let authorizationResult = await AVCaptureDevice.requestAccess(for: .video)
            if authorizationResult {
                await startSession()
            } else {
                print("Camera access not authorized")
            }
        }
    }

    private func startSession() async {
        guard let provider = cameraFrameProvider else { return }

        do {
            try await session?.run([provider])
            await processFrames()
        } catch {
            print("Failed to start session: \(error)")
        }
    }

    private func processFrames() async {
        guard let provider = cameraFrameProvider else { return }

        for await update in provider.cameraFrameUpdates {
            if let sample = update.frame.capturedImage {
                if let cgImage = convertCIImageToCGImage(ciImage: sample) {
                    onFrameProcessed?(cgImage)
                }
            }
        }
    }
    
    private func convertCIImageToCGImage(ciImage: CIImage) -> CGImage? {
        let context = CIContext(options: nil)
        return context.createCGImage(ciImage, from: ciImage.extent)
    }
}