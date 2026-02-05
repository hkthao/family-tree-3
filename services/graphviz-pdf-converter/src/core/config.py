import os

class Config:
    # RabbitMQ Configuration
    RABBITMQ_HOST: str = os.environ.get('RABBITMQ_HOST', 'rabbitmq')
    RABBITMQ_PORT: int = int(os.environ.get('RABBITMQ_PORT', 5672))
    RABBITMQ_USER: str = os.environ.get('RABBITMQ_USER', 'guest')
    RABBITMQ_PASS: str = os.environ.get('RABBITMQ_PASS', 'guest')
    RABBITMQ_HEARTBEAT: int = int(os.environ.get('RABBITMQ_HEARTBEAT', 600)) # seconds
    RABBITMQ_BLOCKED_CONNECTION_TIMEOUT: int = int(os.environ.get('RABBITMQ_BLOCKED_CONNECTION_TIMEOUT', 300)) # seconds

    # Directory Configuration
    INPUT_DIR: str = os.environ.get('INPUT_DIR', '/shared/input')
    OUTPUT_DIR: str = os.environ.get('OUTPUT_DIR', '/shared/output')

    # RabbitMQ Queue and Exchange Names
    RENDER_REQUEST_QUEUE: str = 'render_request'
    RENDER_REQUEST_EXCHANGE: str = 'render_request'
    STATUS_UPDATE_EXCHANGE: str = 'graph_generation_exchange'
    STATUS_UPDATE_ROUTING_KEY: str = 'graph.status.updated'

    # Graphviz Configuration
    RENDER_TIMEOUT_SECONDS: int = int(os.environ.get('RENDER_TIMEOUT_SECONDS', 120)) # Timeout for Graphviz dot command

    # Logging
    LOG_LEVEL: str = os.environ.get('LOG_LEVEL', 'INFO').upper()

config = Config()
