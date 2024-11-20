import SwiftUI
import AVKit
import UnityFramework

struct ProtocolContentView: View {
    let contentItems: [ContentItem]
    let selectedChecklistItem: CheckItemDefinition?
    
    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 20) {
                // Main content items
                ForEach(contentItems) { item in
                    contentView(for: item)
                }
                
                // Checklist item content
                if let checklistItem = selectedChecklistItem,
                   !checklistItem.contentItems.isEmpty {
                    checklistItemContent(for: checklistItem)
                }
            }
            .padding()
        }
    }
    
    private func checklistItemContent(for checklistItem: CheckItemDefinition) -> some View {
        VStack(alignment: .leading) {
            Divider()
                .padding(.vertical)
            
            Text(checklistItem.text)
                .font(.headline)
            
            ForEach(checklistItem.contentItems) { item in
                contentView(for: item)
            }
        }
    }
    
    @ViewBuilder
    private func contentView(for item: ContentItem) -> some View {
        switch item.contentType.lowercased() {
        case "text":
            textContent(for: item)
        case "image":
            imageContent(for: item)
        case "video":
            videoContent(for: item)
        case "timer":
            timerContent(for: item)
        case "webpage":
            webpageContent(for: item)
        case "pdf":
            pdfContent(for: item)
        default:
            unsupportedContent(for: item)
        }
    }
    
    // MARK: - Content Type Views
    
    private func textContent(for item: ContentItem) -> some View {
        if let text = item.properties["Text"], !text.isEmpty {
            return AnyView(TextContentView(text: text))
        }
        return AnyView(EmptyView())
    }
    
    private func imageContent(for item: ContentItem) -> some View {
        if let url = item.properties["URL"], !url.isEmpty {
            return AnyView(ImageContentView(imageName: url))
        }
        return AnyView(EmptyView())
    }
    
    private func videoContent(for item: ContentItem) -> some View {
        if let url = item.properties["URL"], !url.isEmpty {
            return AnyView(
                Button(action: {
                    CallCSharpCallback("requestVideo|" + url)
                }) {
                    ContentButton(
                        icon: "play.circle.fill",
                        title: "Play Video",
                        subtitle: item.properties["Text"] ?? url
                    )
                }
            )
        }
        return AnyView(EmptyView())
    }
    
    private func timerContent(for item: ContentItem) -> some View {
        if let durationStr = item.properties["durationInSeconds"],
           let duration = Int(durationStr),
           duration > 0 {
            return AnyView(
                Button(action: {
                    CallCSharpCallback("requestTimer|" + durationStr)
                }) {
                    ContentButton(
                        icon: "timer",
                        title: "Start Timer",
                        subtitle: "\(duration / 60) minutes"
                    )
                }
            )
        }
        return AnyView(EmptyView())
    }
    
    private func webpageContent(for item: ContentItem) -> some View {
        if let url = item.properties["URL"], !url.isEmpty {
            return AnyView(
                Button(action: {
                    CallCSharpCallback("requestWebpage|" + url)
                }) {
                    ContentButton(
                        icon: "safari.fill",
                        title: "Open Documentation",
                        subtitle: item.properties["Text"]
                    )
                }
            )
        }
        return AnyView(EmptyView())
    }
    
    private func pdfContent(for item: ContentItem) -> some View {
        if let url = item.properties["URL"], !url.isEmpty {
            return AnyView(
                Button(action: {
                    CallCSharpCallback("requestPDF|" + url)
                }) {
                    ContentButton(
                        icon: "doc.text.fill",
                        title: "Open PDF",
                        subtitle: item.properties["Text"] ?? url
                    )
                }
            )
        }
        return AnyView(EmptyView())
    }
    
    private func unsupportedContent(for item: ContentItem) -> some View {
        AnyView(
            Text("Unsupported content type: \(item.contentType)")
                .foregroundColor(.red)
                .padding()
                .background(Color(.systemGray6))
                .cornerRadius(8)
        )
    }
}

// MARK: - Supporting Views

struct ContentButton: View {
    let icon: String
    let title: String
    let subtitle: String?
    
    var body: some View {
        HStack {
            Image(systemName: icon)
                .font(.largeTitle)
            VStack(alignment: .leading) {
                Text(title)
                    .font(.headline)
                if let subtitle = subtitle {
                    Text(subtitle)
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
        
        Group {
            if let url = URL(string: imageName), UIApplication.shared.canOpenURL(url) {
                remoteImage(url: url)
            } else {
                localImage(name: cleanImageName)
            }
        }
        .frame(maxWidth: .infinity)
        .padding()
        .background(Color(.systemGray6))
        .cornerRadius(8)
    }
    
    private func remoteImage(url: URL) -> some View {
        AsyncImage(url: url) { phase in
            switch phase {
            case .empty:
                ProgressView()
            case .success(let image):
                image
                    .resizable()
                    .scaledToFit()
            case .failure:
                failureImage
            @unknown default:
                EmptyView()
            }
        }
    }
    
    private func localImage(name: String) -> some View {
        Group {
            if let uiImage = UIImage(named: name) {
                Image(uiImage: uiImage)
                    .resizable()
                    .scaledToFit()
            } else {
                failureImage
                    .onAppear {
                        print("⚠️ Failed to load image: \(name)")
                        print("Available assets: \(UIImage.assetNames())")
                    }
            }
        }
    }
    
    private var failureImage: some View {
        Image(systemName: "photo")
            .resizable()
            .scaledToFit()
            .foregroundColor(.gray)
    }
}

// MARK: - Helper Extensions

extension UIImage {
    static func assetNames() -> [String] {
        let bundle = Bundle.main
        let assets = bundle.urls(forResourcesWithExtension: "imageset", subdirectory: "Media.xcassets")?
            .map { $0.deletingPathExtension().lastPathComponent } ?? []
        return assets
    }
}
