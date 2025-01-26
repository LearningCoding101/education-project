#!/bin/bash

# Define variables for image name and container name
IMAGE_NAME="education-course"
CONTAINER_NAME="education-course-container"

# Step 1: Build the Docker image
echo "Building Docker image..."
docker build -t $IMAGE_NAME .

# Step 2: Check if the container is already running and remove it if necessary
if [ $(docker ps -q -f name=$CONTAINER_NAME) ]; then
    echo "Stopping and removing existing container..."
    docker stop $CONTAINER_NAME
    docker rm $CONTAINER_NAME
fi

# Step 3: Run the Docker container
echo "Running Docker container..."
docker run -d -p 8080:8080 --name $CONTAINER_NAME $IMAGE_NAME

# Step 4: Verify the container is running
echo "Container is running on http://localhost:8080"
docker ps

