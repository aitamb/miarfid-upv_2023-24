###############################################################################
# EXPERIMENT 1:                                                               #
#   - Dataset: MNIST                                                          #
#   - Model: 1 hidden layer MLP                                               #
###############################################################################

# Try different:
#   - Learning rate
#   - Optimizer
#   - Batch size

# Import LIBRARIES ############################################################
import torch
import torchvision
import torch.nn as nn
from tqdm import tqdm
import multiprocessing
import torch.optim as optim
import torch.nn.functional as  F
from torch.utils.data import Dataset
from torch.utils.data import DataLoader

# Set DEVICE ##################################################################
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print("Device: ", device)

# Prepare DATA ################################################################
train_set = torchvision.datasets.MNIST('.data/', train=True, download=True)
test_set = torchvision.datasets.MNIST('.data/', train=False, download=True)

print("Train images: ", train_set)

# DATASET Class ###############################################################
class MNIST_dataset(Dataset):
    def __init__(self, data, partition = "train"):
        print("\nLoading MNIST ", partition, " Dataset...")
        self.data = data
        self.partition = partition
        print("\tTotal Len.: ", len(self.data), "\n", 50*"-")

    def __len__(self):
        return len(self.data)
    
    def from_pil_to_tensor(self, image):
        return torchvision.transforms.ToTensor()(image)

    def __getitem__(self, idx):
        image = self.data[idx][0]
        image_tensor = self.from_pil_to_tensor(image)
        image_tensor = image_tensor.view(-1)

        label = torch.tensor(self.data[idx][1])
        label = F.one_hot(label, num_classes=10).float()

        return {"img": image_tensor, "label": label}

train_dataset = MNIST_dataset(train_set, partition="train")
test_dataset = MNIST_dataset(test_set, partition="test")

# LOOP for testing different parameters #######################################
print("\n\n", 50*"-")
for batch_size in [100, 200, 500]:
    for lr in [0.1, 0.01, 0.001, 0.0001]:
        for op in ["SGD", "Adam"]:
            print("- Batch size: ", batch_size, 
                " - Learning rate: ", lr, 
                " - Optimizer: ", op)

            # DATALOADER Class ################################################
            num_workers = multiprocessing.cpu_count()-1
            print("Num workers: ", num_workers)
            train_dataloader = DataLoader(train_dataset, batch_size, 
                                shuffle=True, num_workers=num_workers)
            test_dataloader = DataLoader(test_dataset, batch_size, 
                                shuffle=False, num_workers=num_workers)
        
            # NEURAL NETWORK Class ############################################
            num_classes = 10
            net = nn.Sequential(
                        nn.Linear(784, 1024),
                        nn.ReLU(),
                        nn.Linear(1024, 1024),
                        nn.ReLU(),
                        nn.Linear(1024, 1024),
                        nn.ReLU(),
                        nn.Linear(1024, num_classes)
                        # not using softmax cause crossentropy already uses it
                    )

            # TRAINING settings ###############################################
            criterion = nn.CrossEntropyLoss()
            if op == "Adam":
                optimizer = optim.Adam(net.parameters(), lr=lr, 
                            weight_decay=1e-6)
            elif op == "SGD":
                optimizer = optim.SGD(net.parameters(), lr=lr, 
                            momentum=0.9, weight_decay=1e-6)

            epochs = 25

            # TRAINING function ###############################################
            net.to(device) # load model in GPU (if possible)
            print("\n---- Start Training ----")
            best_accuracy = -1
            best_epoch = 0
            for epoch in range(epochs):
                # TRAIN NETWORK
                train_loss, train_correct = 0, 0
                net.train()
                with tqdm(iter(train_dataloader), desc="Epoch " + str(epoch), 
                        unit="batch") as tepoch:
                    for batch in tepoch:
                        # Values of Dataset Class
                        images = batch["img"].to(device)
                        labels = batch["label"].to(device)

                        # zero the parameter gradients
                        optimizer.zero_grad()

                        # Forward
                        outputs = net(images)
                        loss = criterion(outputs, labels)

                        # Calculate gradients
                        loss.backward()

                        # Update gradients
                        optimizer.step()

                        # one hot -> labels
                        labels = torch.argmax(labels, dim=1)
                        pred = torch.argmax(outputs, dim=1)
                        train_correct += pred.eq(labels).sum().item()

                        # print statistics
                        train_loss += loss.item()

                train_loss /= len(train_dataloader.dataset)

                # TEST NETWORK
                test_loss, test_correct = 0, 0
                net.eval()
                with torch.no_grad():
                    with tqdm(iter(test_dataloader), desc="Test " + str(epoch), 
                            unit="batch") as tepoch:
                        for batch in tepoch:

                            images = batch["img"].to(device)
                            labels = batch["label"].to(device)

                            # Forward
                            outputs = net(images)
                            test_loss += criterion(outputs, labels)

                            # one hot -> labels
                            labels = torch.argmax(labels, dim=1)
                            pred = torch.argmax(outputs, dim=1)

                            test_correct += pred.eq(labels).sum().item()

                test_loss /= len(test_dataloader.dataset)
                test_accuracy = 100. * test_correct / len(test_dataloader.dataset)

                print("[Epoch {}] Train Loss: {:.6f} - Test Loss: {:.6f} \
                    - Train Accuracy: {:.2f}% - Test Accuracy: {:.2f}%"
                    .format(epoch + 1, train_loss, test_loss, 
                            100. * train_correct / len(train_dataloader.dataset), 
                            test_accuracy
                ))

                if test_accuracy > best_accuracy:
                    best_accuracy = test_accuracy
                    best_epoch = epoch

                    # Save best weights
                    torch.save(net.state_dict(), "best_model.pt")
            
            print("\nBEST TEST ACCURACY: ", best_accuracy, " in epoch ", best_epoch)

            # Load best weights
            net.load_state_dict(torch.load("best_model.pt"))

            test_loss, test_correct = 0, 0
            net.eval()
            with torch.no_grad():
                with tqdm(iter(test_dataloader), desc="Test " + str(epoch), unit="batch") as tepoch:
                    for batch in tepoch:

                        images = batch["img"].to(device)
                        labels = batch["label"].to(device)

                        # Forward
                        outputs = net(images)
                        test_loss += criterion(outputs, labels)

                        # one hot -> labels
                        labels = torch.argmax(labels, dim=1)
                        pred = torch.argmax(outputs, dim=1)

                        test_correct += pred.eq(labels).sum().item()

                test_loss /= len(test_dataloader.dataset)
                test_accuracy = 100. * test_correct / len(test_dataloader.dataset)
            print("###### Final best acc: ", test_accuracy, " ######\n\n")
            print(50*"-")