import SwiftUI
import AVKit
import UnityFramework

struct ProtocolContentView: View {
    let contentItems: [ContentItem]
    let selectedChecklistItem: CheckItemDefinition?
    
    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 20) {
                ForEach(contentItems) { item in
                    contentView(for: item)
                }
                
                if let checklistItem = selectedChecklistItem,
                   !checklistItem.contentItems.isEmpty {
                    Divider()
                        .padding(.vertical)
                    
                    Text(checklistItem.text)
                        .font(.headline)
                    
                    ForEach(checklistItem.contentItems) { item in
                        contentView(for: item)
                    }
                }
            }
            .padding()
        }
    }
    
    @ViewBuilder
    private func contentView(for item: ContentItem) -> some View {
        switch item.contentType.lowercased() {
        case "text":
            if let text = item.properties["Text"], !text.isEmpty {
                TextContentView(text: text)
            }
        
        case "image":
            if let url = item.properties["URL"], !url.isEmpty {
                ImageContentView(imageName: url)
            }
        
        case "video":
            if let url = item.properties["URL"], !url.isEmpty {
                Button(action: {
                    CallCSharpCallback("requestVideo|" + url)
                }) {
                    HStack {
                        Image(systemName: "play.circle.fill")
                            .font(.largeTitle)
                        VStack(alignment: .leading) {
                            Text("Play Video")
                                .font(.headline)
                            if let text = item.properties["Text"] {
                                Text(text)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            } else {
                                Text(url)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            }
                        }
                    }
                    .padding()
                    .frame(maxWidth: .infinity)
                    .cornerRadius(8)
                }
            }
        
        case "timer":
            if let durationStr = item.properties["durationInSeconds"],
               let duration = Int(durationStr),
               duration > 0 {
                Button(action: {
                    CallCSharpCallback("requestTimer|" + durationStr)
                }) {
                    HStack {
                        Image(systemName: "timer")
                            .font(.largeTitle)
                        VStack(alignment: .leading) {
                            Text("Start Timer")
                                .font(.headline)
                            Text("\(duration / 60) minutes")
                                .font(.subheadline)
                                .foregroundColor(.secondary)
                        }
                    }
                    .padding()
                    .frame(maxWidth: .infinity)
                    .cornerRadius(8)
                }
            }
        
        case "webpage":
            if let url = item.properties["URL"], !url.isEmpty {
                Button(action: {
                    CallCSharpCallback("requestWebpage|" + url)
                }) {
                    HStack {
                        Image(systemName: "safari.fill")
                            .font(.largeTitle)
                        VStack(alignment: .leading) {
                            Text("Open Documentation")
                                .font(.headline)
                            if let text = item.properties["Text"] {
                                Text(text)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            }
                        }
                    }
                    .padding()
                    .frame(maxWidth: .infinity)
                    .cornerRadius(8)
                }
            }
        
        case "pdf":
            if let url = item.properties["URL"], !url.isEmpty {
                Button(action: {
                    CallCSharpCallback("requestPDF|" + url)
                }) {
                    HStack {
                        Image(systemName: "doc.text.fill")
                            .font(.largeTitle)
                        VStack(alignment: .leading) {
                            Text("Open PDF")
                                .font(.headline)
                            if let text = item.properties["Text"] {
                                Text(text)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            } else {
                                Text(url)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            }
                        }
                    }
                    .padding()
                    .frame(maxWidth: .infinity)
                    .cornerRadius(8)
                }
            }
        
        default:
            Text("Unsupported content type: \(item.contentType)")
                .foregroundColor(.red)
                .padding()
                .background(Color(.systemGray6))
                .cornerRadius(8)
        }
    }
}

struct TextContentView: View {
    let text: String
    
    var body: some View {
        Text(text)
            .font(.body)
            .padding()
            .background(Color(.systemGray6))
            .cornerRadius(8)
    }
}

struct ImageContentView: View {
    let imageName: String
    
    var body: some View {
        let cleanImageName = imageName
            .replacingOccurrences(of: ".png", with: "")
            .replacingOccurrences(of: ".jpg", with: "")
            .replacingOccurrences(of: ".jpeg", with: "")
        
        if let url = URL(string: imageName), UIApplication.shared.canOpenURL(url) {
            // Remote Image
            AsyncImage(url: url) { phase in
                switch phase {
                case .empty:
                    ProgressView()
                case .success(let image):
                    image
                        .resizable()
                        .scaledToFit()
                case .failure:
                    Image(systemName: "photo")
                        .resizable()
                        .scaledToFit()
                        .foregroundColor(.gray)
                @unknown default:
                    EmptyView()
                }
            }
            .frame(maxWidth: .infinity)
            .padding()
            .background(Color(.systemGray6))
            .cornerRadius(8)
        } else {
            // Local image
            if let uiImage = UIImage(named: cleanImageName) {
                Image(uiImage: uiImage)
                    .resizable()
                    .scaledToFit()
                    .frame(maxWidth: .infinity)
                    .padding()
                    .background(Color(.systemGray6))
                    .cornerRadius(8)
            } else {
                Image(systemName: "photo")
                    .resizable()
                    .scaledToFit()
                    .foregroundColor(.gray)
                    .frame(maxWidth: .infinity)
                    .padding()
                    .background(Color(.systemGray6))
                    .cornerRadius(8)
                    .onAppear {
                        print("⚠️ Failed to load image: \(cleanImageName)")
                        print("Available assets: \(UIImage.assetNames())")
                    }
            }
        }
    }
}

// Helper extension for debugging
extension UIImage {
    static func assetNames() -> [String] {
        let bundle = Bundle.main
        let assets = bundle.urls(forResourcesWithExtension: "imageset", subdirectory: "Media.xcassets")?
            .map { $0.deletingPathExtension().lastPathComponent } ?? []
        return assets
    }
}
