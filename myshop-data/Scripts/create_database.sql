-- Database: myshop_db
-- Tạo database (chạy manual hoặc script riêng)
-- CREATE DATABASE myshop_db;

-- Bảng CATEGORY
CREATE TABLE IF NOT EXISTS category (
    category_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT
);

-- Bảng PRODUCT
CREATE TABLE IF NOT EXISTS product (
    product_id SERIAL PRIMARY KEY,
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    import_price INTEGER NOT NULL CHECK (import_price >= 0),
    count INTEGER NOT NULL DEFAULT 0 CHECK (count >= 0),
    description TEXT,
    category_id INTEGER NOT NULL,
    FOREIGN KEY (category_id) REFERENCES category(category_id) ON DELETE RESTRICT
);

-- Bảng ORDER
CREATE TABLE IF NOT EXISTS "order" (
    order_id SERIAL PRIMARY KEY,
    created_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    final_price INTEGER NOT NULL CHECK (final_price >= 0),
    status VARCHAR(50) NOT NULL DEFAULT 'Created' CHECK (status IN ('Created', 'Paid', 'Cancelled'))
);

-- Bảng ORDER_ITEM
CREATE TABLE IF NOT EXISTS order_item (
    order_item_id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_sale_price NUMERIC(10,2) NOT NULL CHECK (unit_sale_price >= 0),
    total_price INTEGER NOT NULL CHECK (total_price >= 0),
    FOREIGN KEY (order_id) REFERENCES "order"(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES product(product_id) ON DELETE RESTRICT
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_product_category_id ON product(category_id);
CREATE INDEX IF NOT EXISTS idx_product_sku ON product(sku);
CREATE INDEX IF NOT EXISTS idx_order_created_time ON "order"(created_time);
CREATE INDEX IF NOT EXISTS idx_order_status ON "order"(status);
CREATE INDEX IF NOT EXISTS idx_order_item_order_id ON order_item(order_id);
CREATE INDEX IF NOT EXISTS idx_order_item_product_id ON order_item(product_id);

