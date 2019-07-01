using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace aspnetcoreoauth.Models
{
    public class SampleTestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] HardProperty { get; set; }
    }

    public class SampleTestEntityValidator : AbstractValidator<SampleTestEntity>
    {
        public SampleTestEntityValidator(IApplicationDBContext context)
        {
            int entityId = 0;
            CascadeMode = CascadeMode.StopOnFirstFailure;
            When(entity => {
                entityId = entity.Id; 
                return true; 
            }, () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MustAsync(async (name, cancellation)
                                    => !((await context
                                            .SampleTestEntity
                                            .Where(t => (entityId > 0 && t.Id != entityId && t.Name.ToLower().Trim() == name.ToLower().Trim()) // while updating exiting entity
                                                        || (entityId == 0 && t.Name.ToLower().Trim() == name.ToLower().Trim())) // while adding new entity
                                            .ToAsyncEnumerable().Count()
                                            ) > 0
                                        )
                    ).WithMessage("Name already exits");
            });
        }
    }
}
