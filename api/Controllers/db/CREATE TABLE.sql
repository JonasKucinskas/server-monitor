CREATE TABLE server_metrics (
    id SERIAL PRIMARY KEY,
    time TIMESTAMPTZ NOT NULL,
    server_id TEXT NOT NULL,
    cpu_name TEXT,
    cpu_freq DOUBLE PRECISION,
    battery_name TEXT,
    battery_capacity INTEGER,
    battery_status TEXT,
    disk_total_space BIGINT,
    disk_used_space BIGINT,
    disk_free_space BIGINT,
    ram_mem_total BIGINT,
    ram_mem_free BIGINT,
    ram_mem_used BIGINT,
    ram_mem_available BIGINT,
    ram_buffers BIGINT,
    ram_cached BIGINT,
    ram_swap_total BIGINT,
    ram_swap_free BIGINT,
    ram_swap_used BIGINT,
    sensor_processor_value INTEGER,
    sensor_processor_name TEXT,
    PRIMARY KEY (time, server_id)
);

SELECT create_hypertable('server_metrics', 'time');

-- cpu_cores table
CREATE TABLE cpu_cores (
    time TIMESTAMPTZ NOT NULL,
    server_id TEXT NOT NULL,
    core_name TEXT NOT NULL,
    total DOUBLE PRECISION,
    "user" DOUBLE PRECISION,
    nice DOUBLE PRECISION,
    system DOUBLE PRECISION,
    idle DOUBLE PRECISION,
    iowait DOUBLE PRECISION,
    irq DOUBLE PRECISION,
    softirq DOUBLE PRECISION,
    steal DOUBLE PRECISION,
    PRIMARY KEY (time, server_id, core_name),
);

SELECT create_hypertable('cpu_cores', 'time', 'core_name');

CREATE TABLE disk_partitions (
    time TIMESTAMPTZ NOT NULL,
    server_id TEXT NOT NULL,
    partition_name TEXT NOT NULL,
    read_speed BIGINT,
    write_speed BIGINT,
    io_time BIGINT,
    weighted_io_time BIGINT,
    PRIMARY KEY (time, server_id, partition_name)
);

SELECT create_hypertable('disk_partitions', 'time');

CREATE TABLE network_metrics (
    time TIMESTAMPTZ NOT NULL,
    server_id TEXT NOT NULL,
    interface_name TEXT NOT NULL,
    upload BIGINT,
    download BIGINT,
    receive_bytes BIGINT,
    receive_packets BIGINT,
    receive_errs BIGINT,
    receive_drop BIGINT,
    receive_fifo BIGINT,
    receive_frame BIGINT,
    receive_compressed BIGINT,
    receive_multicast BIGINT,
    transmit_bytes BIGINT,
    transmit_packets BIGINT,
    transmit_errs BIGINT,
    transmit_drop BIGINT,
    transmit_fifo BIGINT,
    transmit_colls BIGINT,
    transmit_carrier BIGINT,
    transmit_compressed BIGINT,
    PRIMARY KEY (time, server_id, interface_name)
);

SELECT create_hypertable('network_metrics', 'time', 'interface_name');

CREATE TABLE sensors (
    time TIMESTAMPTZ NOT NULL,
    server_id TEXT NOT NULL,
    sensor_name TEXT NOT NULL,
    value DOUBLE PRECISION,
    PRIMARY KEY (time, server_id, sensor_name)
);

SELECT create_hypertable('sensors', 'time');
