using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Services.Interfaces;

public interface IProductService
{
	// Products
	Task<List<ProductResponse>> GetAllProductsAsync();

	Task<ProductResponse?> GetProductByIdAsync(int id);

	Task<(ProductResponse? product, string? error)> CreateProductAsync(
		CreateProductRequest req, int actorId);

	Task<(ProductResponse? product, string? error)> UpdateProductAsync(
		int id, UpdateProductRequest req, int actorId);

	Task<(bool success, string? error)> DeleteProductAsync(
		int id, int actorId);

	// BOM
	Task<List<BOMResponse>> GetBOMsByProductAsync(int productId);

	Task<(BOMResponse? bom, string? error)> CreateBOMAsync(
		int productId, CreateBOMRequest req, int actorId);

	Task<(BOMResponse? bom, string? error)> UpdateBOMAsync(
		int bomId, CreateBOMRequest req, int actorId);

	Task<(bool success, string? error)> ObsoleteBOMAsync(
		int bomId, int actorId);

	// Components
	Task<List<ComponentResponse>> GetAllComponentsAsync();

	Task<(ComponentResponse? component, string? error)> CreateComponentAsync(
		CreateComponentRequest req, int actorId);
}